<template>
  <div class="max-w-2xl mx-auto p-6">
    <h1 class="text-3xl font-bold mb-4">🎧 Transcribe Generator</h1>

    <div class="mb-4">
      <label class="block mb-2 font-medium">Video Kaynağı Türü</label>
      <select v-model="inputType" class="w-full border rounded p-2">
        <option value="youtube">YouTube Linki</option>
        <option value="file">MP3 / MP4 Dosyası</option>
      </select>
    </div>

    <div class="mb-4" v-if="inputType === 'youtube'">
      <label class="block mb-2 font-medium">YouTube Linki</label>
      <input
        v-model="youtubeLink"
        type="text"
        placeholder="https://youtube.com/..."
        class="w-full border rounded p-2"
      />
    </div>

    <div class="mb-4" v-else>
      <label class="block mb-2 font-medium">Ses / Video Dosyası</label>
      <input
        type="file"
        @change="handleFileUpload"
        accept=".mp3,.mp4"
        class="w-full border rounded p-2"
      />
    </div>

    <div class="mb-4">
      <label class="block mb-2 font-medium">Whisper Model Seçimi</label>
      <select v-model="selectedModel" class="w-full border rounded p-2">
        <option :value="0">Tiny</option>
        <option :value="1">Base</option>
        <option :value="2">Small</option>
        <option :value="3">Medium</option>
        <option :value="4">Large</option>
      </select>
    </div>

    <button @click="submit" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
      Gönder
    </button>

    <div class="mt-6">
      <label class="block mb-2 font-medium">Transcript</label>
      <textarea v-model="transcript" readonly class="w-full border rounded p-2 h-48"></textarea>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import axios from "axios";

const inputType = ref("youtube");
const youtubeLink = ref("");
const selectedFile = ref(null);
const transcript = ref("");
const selectedModel = ref(0);

const handleFileUpload = (event) => {
  selectedFile.value = event.target.files[0];
};

const submit = async () => {
  transcript.value = "İşleniyor...";

  if (inputType.value === "youtube") {
    try {
      const response = await axios.post("/api/transcribe/youtube", {
        url: youtubeLink.value,
        model: selectedModel.value,
      });
      transcript.value = response.data.transcript;
    } catch (error) {
      console.error("YouTube transcription error:", error);
      transcript.value = "Bir hata oluştu (YouTube).";
    }
  } else {
    if (!selectedFile.value) {
      transcript.value = "Lütfen bir dosya seçin.";
      return;
    }

    const formData = new FormData();
    formData.append("file", selectedFile.value);
    formData.append("model", selectedModel.value);

    try {
      const response = await axios.post("/api/transcribe/file", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      transcript.value = response.data.transcript;
    } catch (error) {
      console.error("File transcription error:", error);
      transcript.value = "Bir hata oluştu (dosya).";
    }
  }
};
</script>
