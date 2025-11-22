# GPU Setup Guide for Windows

## Prerequisites

### 1. Install NVIDIA GPU Drivers
- Download from: https://www.nvidia.com/Download/index.aspx
- Install the latest driver for your GPU

### 2. Install NVIDIA Container Toolkit for Windows

#### For Docker Desktop (Recommended for Windows):
1. Install **Docker Desktop for Windows** (latest version)
2. Enable **WSL 2** backend in Docker Desktop settings
3. Install **NVIDIA CUDA on WSL 2**:
   ```powershell
   # In PowerShell (Admin)
   wsl --install
   wsl --set-default-version 2
   ```

4. Inside WSL 2, install NVIDIA Container Toolkit:
   ```bash
   # In WSL 2 terminal
   distribution=$(. /etc/os-release;echo $ID$VERSION_ID)
   curl -fsSL https://nvidia.github.io/libnvidia-container/gpgkey | sudo gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg
   curl -s -L https://nvidia.github.io/libnvidia-container/$distribution/libnvidia-container.list | \
     sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | \
     sudo tee /etc/apt/sources.list.d/nvidia-container-toolkit.list
   
   sudo apt-get update
   sudo apt-get install -y nvidia-container-toolkit
   sudo nvidia-ctk runtime configure --runtime=docker
   sudo systemctl restart docker
   ```

### 3. Verify GPU Access
```bash
# Test if Docker can see your GPU
docker run --rm --gpus all nvidia/cuda:12.1.0-base-ubuntu22.04 nvidia-smi
```

You should see your GPU listed!

## Running with GPU

### Mac / Linux / CPU-only Systems (Default)
```bash
docker-compose up --build -d
```

### Windows with NVIDIA GPU
```bash
docker-compose -f docker-compose.gpu.yml up --build -d
```

This uses `Dockerfile.gpu` and `environment.gpu.yml` which include CUDA support.

## Performance Comparison

| Mode | Speed | Model Load Time | Transcription (1 min audio) |
|------|-------|-----------------|----------------------------|
| **GPU (NVIDIA)** | 🚀 Very Fast | ~5 seconds | ~10 seconds |
| **CPU** | 🐌 Slow | ~10 seconds | ~2-5 minutes |

## Troubleshooting

### Error: "could not select device driver"
- Make sure NVIDIA drivers are installed
- Restart Docker Desktop
- Verify WSL 2 integration is enabled

### Error: "nvidia-smi not found"
- Install CUDA toolkit in WSL 2
- Restart your computer

### Still using CPU instead of GPU?
Check the logs:
```bash
docker-compose logs python-service
```

You should see: `🚀 Using GPU: NVIDIA GeForce RTX ...`  
If you see: `⚠️ GPU not available, using CPU` - GPU setup is not complete.

## Switching Between GPU/CPU

Edit `docker-compose.yml`:
- **Enable GPU**: Uncomment the `deploy` section
- **Disable GPU**: Comment out the `deploy` section

Then rebuild:
```bash
docker-compose up --build -d
```
