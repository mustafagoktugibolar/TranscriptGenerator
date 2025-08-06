# TranscriptGenerator

TranscriptGenerator is a full-stack, self-hostable web application that automates audio/video transcription using OpenAI Whisper. It features a Python-based Whisper backend for accurate transcription, an ASP.NET Core (C#) server for API orchestration, and a Vue.js frontend for managing file uploads and transcription workflows. It supports multi-format exports (TXT, SRT, VTT, PDF) and is licensed under GPL v3.

## Features

- Upload audio/video files via the web UI
- Transcribe media using OpenAI Whisper (runs locally)
- Export results in multiple formats: .txt, .srt, .vtt, .pdf
- Decoupled frontend/backend architecture
- No third-party API dependency
- Fully open-source and self-hostable

## Architecture

[Vue.js Frontend] <-> [ASP.NET Core API] <-> [Python Whisper Transcriber]
                                          |
                                      [Local Media Files]

## Tech Stack

- Frontend: Vue.js
- Backend: ASP.NET Core (C#)
- Transcriber: Python 3 + OpenAI Whisper
- Formats: .mp3, .mp4 input → .txt, .srt, .vtt, .pdf output
- License: GNU GPL v3

## Prerequisites

- .NET 7 SDK or newer
- Node.js 16+
- Python 3.10+
- ffmpeg (required by Whisper)
- (Optional) CUDA if using GPU with Whisper

## Installation

1. Clone the repository:
   git clone https://github.com/mustafagoktugibolar/TranscriptGenerator.git
   cd TranscriptGenerator

2. Backend setup:
   cd TranscriptGenerator.Server
   dotnet restore
   dotnet run
   # Backend runs at http://localhost:5000

3. Frontend setup:
   cd ../transcriptgenerator.client
   npm install
   npm run serve
   # Frontend runs at http://localhost:8080

4. Python (Whisper) setup:
   cd ../TranscriptGenerator.Server/Python
   python -m venv .venv
   source .venv/bin/activate  # On Windows: .venv\Scripts\activate
   pip install -r requirements.txt

## File Structure

TranscriptGenerator/
├── TranscriptGenerator.Server/       # Backend (.NET)
│   └── Python/                       # Whisper-based transcription logic
├── transcriptgenerator.client/       # Frontend (Vue.js)
├── README.md
└── LICENSE

## Whisper Details

The app uses OpenAI Whisper to process media files into transcriptions. It supports multilingual input and runs on CPU or GPU depending on your environment. You can customize the model size or language in the Python scripts.

Supported input formats: .mp3, .wav, .m4a, .webm, .mp4, .mkv, .avi  
Supported output formats: .txt, .srt, .vtt, .pdf

## Example Flow

1. Upload media from the browser.
2. Backend queues the file.
3. Python processes the transcription with Whisper.
4. Output is stored and made available to download.

## License

This project is licensed under the MIT License. See LICENSE for details.

## Contributing

Feel free to open issues or submit PRs. Contributions may include:
- Adding languages or models
- Improving the frontend
- Adding Docker support
- Enhancing file management or job queueing

## Contact

Created by Göktuğ İbolar  
GitHub: https://github.com/mustafagoktugibolar  
Email: goktugibolar@icloud.com
