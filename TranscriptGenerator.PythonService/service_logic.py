import os
import uuid
import time
import torch
import whisper
import yt_dlp
import shutil

# Global model cache
_model_cache = {}

def get_model(model_name="base"):
    if model_name not in _model_cache:
        print(f"Loading Whisper model: {model_name}...")
        
        # Check GPU availability
        if torch.cuda.is_available():
            device = "cuda"
            gpu_name = torch.cuda.get_device_name(0)
            print(f"🚀 GPU detected: {gpu_name}")
            print(f"   Using CUDA for acceleration")
        else:
            device = "cpu"
            print(f"⚠️  No GPU detected, using CPU")
            print(f"   Note: Transcription will be slower on CPU")
        
        _model_cache[model_name] = whisper.load_model(model_name).to(device)
        print(f"✅ Model '{model_name}' loaded on {device.upper()}")
    return _model_cache[model_name]

def download_youtube_audio(url: str, output_dir: str = "/tmp") -> str:
    """
    Downloads audio from YouTube URL and returns the path to the MP3 file.
    """
    filename = f"temp_{uuid.uuid4().hex}"
    
    ydl_opts = {
        'format': 'bestaudio/best',
        'outtmpl': os.path.join(output_dir, f'{filename}.%(ext)s'),
        'postprocessors': [{
            'key': 'FFmpegExtractAudio',
            'preferredcodec': 'mp3',
            'preferredquality': '192',
        }],
        'quiet': True,
        'no_warnings': True,
    }

    with yt_dlp.YoutubeDL(ydl_opts) as ydl:
        ydl.download([url])

    # The file will be saved with .mp3 extension
    final_path = os.path.join(output_dir, f"{filename}.mp3")
    
    if not os.path.exists(final_path):
        raise Exception("Download failed, file not found.")
        
    return final_path

def transcribe_audio(file_path: str, model_name: str = "base") -> str:
    """
    Transcribes the audio file using Whisper.
    """
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"File not found: {file_path}")

    model = get_model(model_name)
    result = model.transcribe(file_path)
    return result["text"]
