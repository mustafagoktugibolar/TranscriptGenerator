# TranscriptGenerator - Docker Setup

## Quick Start

```bash
docker-compose up --build -d
```

Access at: `http://localhost:5001`

**That's it!** Works on Mac, Linux, and Windows. Automatically uses GPU if available.

## What Happens on Startup?

### 1. GPU Detection (Automatic)

The Python service automatically detects your hardware:

**With GPU (Windows/Linux with NVIDIA):**
```
🚀 GPU detected: NVIDIA GeForce RTX 3080
   Using CUDA for acceleration
✅ Model 'base' loaded on CUDA
```

**Without GPU (Mac/CPU-only systems):**
```
⚠️  No GPU detected, using CPU
   Note: Transcription will be slower on CPU
✅ Model 'base' loaded on CPU
```

### 2. Model Preloading

Whisper model loads on startup (~5-10 seconds):
```
🚀 Preloading Whisper model...
100%|███████████████████| 139M/139M [00:05<00:00, 24.4MiB/s]
✅ Whisper model loaded successfully!
```

### 3. Ready!

All services start:
- ✅ Nginx (Frontend) - `http://localhost:5001`
- ✅ .NET API (Backend)
- ✅ Python Service (AI) - GPU/CPU auto-detected

## GPU Setup (Windows/Linux Only)

If you have an NVIDIA GPU and want to use it:

1. Install NVIDIA drivers
2. Install NVIDIA Container Toolkit (see [`GPU_SETUP.md`](GPU_SETUP.md))
3. Run the same command:
   ```bash
   docker-compose up --build -d
   ```

PyTorch will automatically detect and use your GPU! No config changes needed.

## Performance

| Hardware | Model Load | Transcription (1 min audio) |
|----------|-----------|----------------------------|
| **NVIDIA GPU** | ~5s | ~10 seconds ⚡ |
| **CPU (Mac M1)** | ~10s | ~2-3 minutes |
| **CPU (Intel)** | ~10s | ~3-5 minutes 🐌 |

## Features

✅ **Automatic GPU Detection** - No configuration needed  
✅ **No Cold Start** - Model preloaded on startup  
✅ **Concurrent Requests** - Multiple tabs work simultaneously  
✅ **Cross-Platform** - Works on Mac, Linux, Windows  

## Common Commands

```bash
# Start
docker-compose up --build -d

# Stop
docker-compose down

# View logs
docker-compose logs -f python-service

# Restart just Python service
docker-compose restart python-service
```

## Troubleshooting

### GPU not detected on Windows?

1. Check Docker Desktop has WSL 2 backend enabled
2. Verify NVIDIA drivers: `nvidia-smi` in WSL 2
3. Check logs: `docker-compose logs python-service`
4. See [`GPU_SETUP.md`](GPU_SETUP.md) for detailed setup

### Still slow?

Check the logs to confirm GPU is being used:
```bash
docker-compose logs python-service | grep GPU
```

You should see: `🚀 GPU detected: NVIDIA ...`
