use rodio::{Decoder, OutputStream, OutputStreamBuilder, Sink, Source};
use std::{
    ffi::{c_char, c_int, c_uchar, CStr},
    fs::File,
    io::BufReader,
    sync::{
        atomic::{AtomicBool, Ordering},
        Arc, LazyLock, Mutex,
    },
};

static STREAM: LazyLock<Arc<Mutex<Option<OutputStream>>>> =
    LazyLock::new(|| Arc::new(Mutex::new(None)));

static SINK: LazyLock<Arc<Mutex<Option<Sink>>>> = LazyLock::new(|| Arc::new(Mutex::new(None)));

static FILE_PATH: LazyLock<Arc<Mutex<String>>> =
    LazyLock::new(|| Arc::new(Mutex::new(String::from("NULL"))));

static PLAYBACK_STATUS: AtomicBool = AtomicBool::new(false);

#[unsafe(no_mangle)]
pub extern "C" fn audio_engine_init() -> c_int {
    let stream_handle: rodio::OutputStream = match OutputStreamBuilder::open_default_stream() {
        Ok(stream) => stream,
        Err(_) => return -1,
    };

    match STREAM.lock() {
        Ok(mut global_stream) => {
            *global_stream = Some(stream_handle);
            return 0;
        }
        Err(_) => return -1,
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn get_playback_status() -> c_uchar {
    // 2 = Error
    // 1 = true (done)
    // 0 = false (not done)
    match SINK.lock() {
        Ok(sink_guard) => {
            if let Some(global_sink) = &*sink_guard {
                let is_empty = global_sink.empty();
                return (!is_empty) as c_uchar;
            } else {
                return 2;
            }
        }
        Err(_) => return 2,
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn audio_engine_deinit() {
    stop();
    if let Ok(mut global_sink) = SINK.lock() {
        *global_sink = None;
    }

    if let Ok(mut global_stream) = STREAM.lock() {
        *global_stream = None;
    }
}

// 0: Success
// -1: General error (I/O, lock, stream failure)
// -2: Invalid start position (beyond file length)

#[unsafe(no_mangle)]
pub extern "C" fn play(start_position_ms : c_int) -> c_int {
    let file_path = match FILE_PATH.lock() {
        Ok(global_file_path) => global_file_path.clone(),
        Err(_) => return -1,
    };

    let file = match File::open(&*file_path) {
        Ok(f) => BufReader::new(f),
        Err(_) => return -1,
    };

    let decoder = match Decoder::new(file) {
        Ok(d) => d,
        Err(_) => return -1,
    };

    let start_duration = std::time::Duration::from_millis(start_position_ms as u64);

    let source: Box<dyn Source<Item = f32> + Send> = match decoder.total_duration() {
        Some(total) if start_duration < total => {
            Box::new(decoder.skip_duration(start_duration))
        },
        Some(_) => {
            return -2;
        },
        None => {
            Box::new(decoder.skip_duration(start_duration))
        }
    };

    match STREAM.lock() {
        Ok(global_stream) => {
            if let Some(stream) = &*global_stream {
                let sink = Sink::connect_new(stream.mixer());
                sink.append(source);

                if PLAYBACK_STATUS.load(Ordering::SeqCst) {
                    stop();
                }

                sink.play();

                match SINK.lock() {
                    Ok(mut global_sink) => {
                        *global_sink = Some(sink);
                        PLAYBACK_STATUS.store(true, Ordering::SeqCst);

                        return 0;
                    }
                    Err(_) => return -1,
                }
            } else {
                return -1;
            }
        }
        Err(_) => return -1,
    };
}

#[unsafe(no_mangle)]
pub extern "C" fn stop() -> c_int {
    match SINK.lock() {
        Ok(sink_guard) => {
            if let Some(global_sink) = &*sink_guard {
                global_sink.stop();
                PLAYBACK_STATUS.store(false, Ordering::SeqCst);

                return 0;
            } else {
                return -1;
            }
        }
        Err(_) => return -1,
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn set_file_path(new_file_path: *const c_char) -> c_int {
    if new_file_path.is_null() {
        return -1;
    }

    match FILE_PATH.lock() {
        Ok(mut global_file_path) => {
            let c_string: &CStr = unsafe { CStr::from_ptr(new_file_path) };

            match c_string.to_str() {
                Ok(rust_str) => {
                    *global_file_path = rust_str.to_string();
                    return 0;
                }
                Err(_) => return -1,
            }
        }
        Err(_) => return -1,
    }
}
