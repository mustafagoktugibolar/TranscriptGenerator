import argparse
import whisper
import sys
import io
import torch
import time
import os
os.environ["PATH"] += os.pathsep + "/opt/homebrew/bin"

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

parser = argparse.ArgumentParser()
parser.add_argument('--path', required=True, help="Path to the audio file")
parser.add_argument('--model', default='base', help="Whisper model to use: tiny, base, small, medium, large, turbo")
args = parser.parse_args()

if not os.path.exists(args.path):
    print(f"Error: File not found → {args.path}", file=sys.stderr)
    exit(1)

try:
    start_time = time.time()

    model = whisper.load_model(args.model)
    if torch.cuda.is_available():
        model = model.to("cuda")
        print("🚀 Using GPU:", torch.cuda.get_device_name(0), file=sys.stderr)
    else:
        print("⚠️ GPU not available, using CPU", file=sys.stderr)

    print("🎙️ Transcribing...", file=sys.stderr)
    result = model.transcribe(args.path)

    print("✅ Transcription complete in", round(time.time() - start_time, 2), "seconds", file=sys.stderr)
    print(result["text"])

except Exception as e:
    print(f"❌ Error during transcription: {e}", file=sys.stderr)
    exit(1)
