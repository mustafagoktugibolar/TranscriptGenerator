import argparse, subprocess, uuid

parser = argparse.ArgumentParser()
parser.add_argument('--url', required=True)
args = parser.parse_args()

filename = f"temp_{uuid.uuid4().hex}.mp3"

result = subprocess.run([
    "yt-dlp", "-x", "--audio-format", "mp3", "-o", filename, args.url
], capture_output=True)

if result.returncode != 0:
    print(result.stderr.decode(), file=sys.stderr)
    exit(1)

print(filename)
