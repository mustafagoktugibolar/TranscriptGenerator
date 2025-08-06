import argparse
import subprocess
import uuid
import sys
import os
import time

# Start timer
start_time = time.time()
parser = argparse.ArgumentParser()
parser.add_argument("--url", required=True)
args = parser.parse_args()

# Generate a temp filename
filename = f"temp_{uuid.uuid4().hex}.%(ext)s"

yt_dlp_path = "/opt/homebrew/bin/yt-dlp"
# Build yt-dlp command
command = [
    yt_dlp_path,  
    "--quiet",
    "--no-warnings",
    "-f", "bestaudio",
    "-x",
    "--ffmpeg-location", "/opt/homebrew/bin",
    "--audio-format", "mp3",
    "-o", filename,
    args.url
]

# Run download
result = subprocess.run(command, capture_output=True)

# Error handling
if result.returncode != 0:
    print(result.stderr.decode(), file=sys.stderr)
    exit(1)

# Final resolved filename
final_filename = filename.replace("%(ext)s", "mp3")

# Stop timer
elapsed = round(time.time() - start_time, 2)

# Print absolute path and time
print(os.path.abspath(final_filename))
print(f"⏱️ Download completed in {elapsed} seconds", file=sys.stderr)
