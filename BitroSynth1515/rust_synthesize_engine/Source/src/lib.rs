use hound;
use rand::prelude::*;
use std::f32::consts;
use std::ffi::{CStr, c_char, c_int, c_uchar};
use std::str::FromStr;
use std::sync::atomic::{AtomicI32, Ordering};

static SAMPLE_RATE: AtomicI32 = AtomicI32::new(11025);

#[unsafe(no_mangle)]
pub extern "C" fn set_sample_rate(new_sample_rate: c_int) {
    SAMPLE_RATE.store(new_sample_rate, Ordering::SeqCst);
}

fn generate_triangle(milliseconds: u32, frequency: f32) -> Vec<f32> {
    let sample_rate: f32 = SAMPLE_RATE.load(Ordering::SeqCst) as f32;
    let seconds: f32 = milliseconds as f32 / 1000.0;
    let total_samples: usize = (seconds * sample_rate) as usize;
    let mut output: Vec<f32> = Vec::with_capacity(total_samples);

    for n in 0..total_samples {
        let t: f32 = n as f32 / sample_rate;
        let value: f32 = 4.0 * ((frequency * t - 0.25).fract() - 0.5).abs() - 1.0;
        output.push(value);
    }

    return output;
}

fn generate_square(milliseconds: u32, frequency: f32) -> Vec<f32> {
    let sample_rate: f32 = SAMPLE_RATE.load(Ordering::SeqCst) as f32;
    let seconds: f32 = milliseconds as f32 / 1000.0;
    let total_samples: usize = (seconds * sample_rate) as usize;
    let mut output: Vec<f32> = Vec::with_capacity(total_samples);

    for n in 0..total_samples {
        let t: f32 = n as f32 / sample_rate;
        let value: f32 = if (2.0 * std::f32::consts::PI * frequency * t).sin() >= 0.0 {
            1.0
        } else {
            -1.0
        };
        output.push(value);
    }

    return output;
}

fn generate_sine(milliseconds: u32, frequency: f32) -> Vec<f32> {
    let sample_rate: f32 = SAMPLE_RATE.load(Ordering::SeqCst) as f32;
    let seconds: f32 = milliseconds as f32 / 1000.0;
    let total_samples: usize = (seconds * sample_rate) as usize;
    let mut output: Vec<f32> = Vec::with_capacity(total_samples);

    for n in 0..total_samples {
        let t = n as f32 / sample_rate;
        let value = (2.0 * consts::PI * frequency * t).sin();
        output.push(value);
    }

    return output;
}

fn generate_sawtooth(milliseconds: u32, frequency: f32) -> Vec<f32> {
    let sample_rate: f32 = SAMPLE_RATE.load(Ordering::SeqCst) as f32;
    let seconds: f32 = milliseconds as f32 / 1000.0;
    let total_samples: usize = (seconds * sample_rate) as usize;
    let mut output: Vec<f32> = Vec::with_capacity(total_samples);

    for n in 0..total_samples {
        let t: f32 = n as f32 / sample_rate;
        let value: f32 = 2.0 * (frequency * t - (frequency * t).floor()) - 1.0;
        output.push(value);
    }

    return output;
}

fn generate_pink_noise(milliseconds: u32) -> Vec<f32> {
    let mut b0: f32 = 0.0;
    let mut b1: f32 = 0.0;
    let mut b2: f32 = 0.0;
    let mut b3: f32 = 0.0;
    let mut b4: f32 = 0.0;
    let mut b5: f32 = 0.0;
    let mut b6: f32 = 0.0;

    let input: Vec<f32> = generate_noise(milliseconds);

    let mut output_data: Vec<f32> = Vec::with_capacity(input.len());

    for x in input {
        b0 = 0.99886 * b0 + x * 0.0555179;
        b1 = 0.99332 * b1 + x * 0.0750759;
        b2 = 0.96900 * b2 + x * 0.1538520;
        b3 = 0.86650 * b3 + x * 0.3104856;
        b4 = 0.55000 * b4 + x * 0.5329522;
        b5 = -0.7616 * b5 - x * 0.0168980;
        let y: f32 = b0 + b1 + b2 + b3 + b4 + b5 + b6 + x * 0.5362;
        b6 = x * 0.115926;
        output_data.push(y);
    }

    let max_amp: f32 = output_data
        .iter()
        .cloned()
        .fold(0.0f32, |a, b| a.max(b.abs()));

    if max_amp > 1.0 {
        output_data.iter_mut().for_each(|s| *s /= max_amp);
    }

    return output_data;
}

fn generate_noise(milliseconds: u32) -> Vec<f32> {
    let mut rng: ThreadRng = rand::rng();
    let seconds: f32 = milliseconds as f32 / 1000.0;

    let sample_rate = SAMPLE_RATE.load(Ordering::SeqCst) as f32;
    let total_samples_length = (seconds * sample_rate) as usize;
    let mut output_data: Vec<f32> = Vec::new();
    for _ in 0..total_samples_length {
        output_data.push(rng.random_range(-1.0..=1.0));
    }
    return output_data;
}

fn generate_void(milliseconds: u32) -> Vec<f32> {
    let sample_rate: f32 = SAMPLE_RATE.load(Ordering::SeqCst) as f32;

    return vec![0.0; (milliseconds as f32 / 1000.0 * sample_rate) as usize];
}

fn parse_number<T: FromStr>(s: &str) -> Option<T> {
    return s.replace(' ', "").replace(',', ".").parse::<T>().ok();
}

fn generate_channel(input: String) -> Option<Vec<f32>> {
    let notes: Vec<&str> = input.split('>').collect();
    let mut output_data: Vec<f32> = Vec::new();
    for i in 0..notes.len() {
        let note: Vec<&str> = notes[i].split('_').collect();
        if note.len() < 4 {
            return None;
        }

        let frequency: f32 = match parse_number::<f32>(note[0]) {
            Some(o) => o,
            None => return None,
        };

        let milliseconds: u32 = match parse_number::<u32>(note[1]) {
            Some(o) => o,
            None => return None,
        };

        let gain: f32 = match parse_number::<f32>(note[2]) {
            Some(o) => o,
            None => return None,
        };

        let wave_form = note[3].to_ascii_uppercase();

        let wave_form_type: &str = &wave_form.replace(' ', "");

        let mut wave: Vec<f32> = match wave_form_type {
            "TRI" => generate_triangle(milliseconds, frequency),
            "SINE" => generate_sine(milliseconds, frequency),
            "SQ" => generate_square(milliseconds, frequency),
            "SAW" => generate_sawtooth(milliseconds, frequency),
            "NOISE" => generate_noise(milliseconds),
            "PINK" => generate_pink_noise(milliseconds),
            "VOID" => generate_void(milliseconds),
            _ => return None,
        };

        for sample in wave.iter_mut() {
            *sample = (*sample * gain).clamp(-1.0, 1.0);
        }

        output_data.extend(wave);
    }

    return Some(output_data);
}

fn write_wav(path: String, samples: &[f32], bit_8: c_uchar) -> c_int {
    // 1 = true
    // 0 = false
    if bit_8 == 1 {
        // 8 Bit
        let spec: hound::WavSpec = hound::WavSpec {
            channels: 1,
            sample_rate: SAMPLE_RATE.load(Ordering::SeqCst) as u32,
            bits_per_sample: 8,
            sample_format: hound::SampleFormat::Int,
        };

        let mut writer = match hound::WavWriter::create(path, spec) {
            Ok(o) => o,
            Err(_) => return -1,
        };
        for &s in samples {
            let val: i8 = (s * i8::MAX as f32) as i8;
            match writer.write_sample(val) {
                Ok(_o) => {}
                Err(_) => return -1,
            };
        }
        match writer.finalize() {
            Ok(_) => return 0,
            Err(_) => return -1,
        };
    } else {
        // 16 Bit
        let spec: hound::WavSpec = hound::WavSpec {
            channels: 1,
            sample_rate: SAMPLE_RATE.load(Ordering::SeqCst) as u32,
            bits_per_sample: 16,
            sample_format: hound::SampleFormat::Int,
        };

        let mut writer = match hound::WavWriter::create(path, spec) {
            Ok(o) => o,
            Err(_) => return -1,
        };

        for &s in samples {
            let val = (s * i16::MAX as f32) as i16;
            match writer.write_sample(val) {
                Ok(_o) => {}
                Err(_) => return -1,
            };
        }

        match writer.finalize() {
            Ok(_) => return 0,
            Err(_) => return -1,
        };
    }
}

fn c_char_to_string(c_str_ptr: *const c_char) -> Option<String> {
    if c_str_ptr.is_null() {
        return None;
    }
    unsafe {
        let c_str = CStr::from_ptr(c_str_ptr);
        match c_str.to_str() {
            Ok(str_slice) => Some(str_slice.to_string()),
            Err(_) => None,
        }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn synthesize(
    input_1: *const c_char,
    input_2: *const c_char,
    input_3: *const c_char,
    input_4: *const c_char,
    bit_8: c_uchar,
    output_path: *const c_char,
) -> c_int {
    let s1: String = match c_char_to_string(input_1) {
        Some(s) => s,
        None => return -1,
    };
    let s2: String = match c_char_to_string(input_2) {
        Some(s) => s,
        None => return -1,
    };
    let s3: String = match c_char_to_string(input_3) {
        Some(s) => s,
        None => return -1,
    };
    let s4: String = match c_char_to_string(input_4) {
        Some(s) => s,
        None => return -1,
    };
    let string_output_path: String = match c_char_to_string(output_path) {
        Some(s) => s,
        None => return -1,
    };

    let channel_1: Vec<f32> = if s1.is_empty() {
        Vec::new()
    } else {
        generate_channel(s1).unwrap_or_default()
    };
    let channel_2: Vec<f32> = if s2.is_empty() {
        Vec::new()
    } else {
        generate_channel(s2).unwrap_or_default()
    };
    let channel_3: Vec<f32> = if s3.is_empty() {
        Vec::new()
    } else {
        generate_channel(s3).unwrap_or_default()
    };
    let channel_4: Vec<f32> = if s4.is_empty() {
        Vec::new()
    } else {
        generate_channel(s4).unwrap_or_default()
    };

    let channels: [&Vec<f32>; 4] = [&channel_1, &channel_2, &channel_3, &channel_4];

    let enabled_channel_number : u8 = channels
        .iter()
        .filter(|c| !c.is_empty())
        .count() as u8;

    if enabled_channel_number == 0 {
        return -1;
    }

    let max_length: usize = channels
        .iter()
        .map(|c: &&Vec<f32>| c.len())
        .max()
        .unwrap_or(0);

    let mut output_data: Vec<f32> = Vec::with_capacity(max_length);

    for i in 0..max_length {
        let mut sum = 0.0;
        let mut active_channels = 0;

        for c in &channels {
            let sample = c.get(i).copied().unwrap_or(0.0);
            sum += sample;
            if sample != 0.0 {
                active_channels += 1;
            }
        }

        if active_channels > 0 {
            output_data.push(sum / active_channels as f32);
        } else {
            output_data.push(0.0);
        }
    }

    return write_wav(string_output_path, &output_data, bit_8);
}