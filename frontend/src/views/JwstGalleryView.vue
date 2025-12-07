<template>
  <div class="min-h-screen bg-gradient-to-b from-slate-950 via-slate-900 to-slate-800 text-slate-100 pt-20">
    <div class="max-w-7xl mx-auto px-4 py-8">
      <div class="bg-white/3 rounded-2xl shadow-lg overflow-hidden border border-white/6">
        <div class="px-6 py-4 border-b border-white/6 flex flex-col md:flex-row md:items-center md:justify-between gap-3">
          <h2 class="text-lg font-semibold text-slate-100">JWST — последние изображения</h2>

          <form @submit.prevent="onSubmit" class="flex flex-wrap items-center gap-2">
            <select v-model="form.source" class="text-sm py-1 px-2 rounded border bg-slate-800 text-slate-100">
              <option value="jpg">Все JPG</option>
              <option value="suffix">По суффиксу</option>
              <option value="program">По программе</option>
            </select>

            <input v-if="form.source === 'suffix'" v-model="form.suffix" type="text" placeholder="_cal / _thumb" class="text-sm py-1 px-2 rounded border bg-slate-800 text-slate-100" />

            <input v-if="form.source === 'program'" v-model="form.program" type="text" placeholder="2734" class="text-sm py-1 px-2 rounded border bg-slate-800 text-slate-100" />

            <select v-model="form.instrument" class="text-sm py-1 px-2 rounded border bg-slate-800 text-slate-100">
              <option value="">Любой инструмент</option>
              <option>NIRCam</option><option>MIRI</option><option>NIRISS</option><option>NIRSpec</option><option>FGS</option>
            </select>

            <select v-model.number="form.perPage" class="text-sm py-1 px-2 rounded border bg-slate-800 text-slate-100">
              <option :value="12">12</option>
              <option :value="24">24</option>
              <option :value="36">36</option>
              <option :value="48">48</option>
            </select>

            <button type="submit" class="text-sm py-1 px-3 rounded bg-indigo-600 hover:bg-indigo-500 text-white">Показать</button>
          </form>
        </div>

        <div class="p-6">
          <div class="flex items-center justify-between mb-4 text-slate-300">
            <div>Источник: <span class="font-mono text-indigo-200">{{ feed.source || '—' }}</span></div>
            <div>Показано: <span class="font-medium text-white">{{ feed.count ?? items.length }}</span></div>
          </div>

          <div v-if="loading" class="p-8 text-center text-slate-300">Загрузка…</div>
          <div v-else-if="error" class="p-4 text-rose-300 bg-rose-900/10 rounded">{{ error }}</div>

          <div v-else>
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
              <template v-for="(it, i) in items" :key="it.url + i">
                <figure @click="openPreview(it)" class="group bg-slate-900/40 rounded-lg overflow-hidden border border-white/6 shadow-sm relative cursor-pointer transform transition hover:scale-[1.02]">
                  <div class="w-full aspect-[4/3] overflow-hidden bg-slate-800">
                    <img :src="it.url" :alt="it.caption || it.obs || 'JWST image'" loading="lazy" class="w-full h-full object-cover transform transition-transform duration-300 group-hover:scale-105" />
                  </div>

                  <figcaption class="p-3">
                    <div class="text-sm font-medium text-white line-clamp-2">{{ it.caption || it.obs }}</div>
                    <div class="mt-1 text-xs text-slate-400 flex items-center justify-between">
                      <div>{{ it.program ? 'P' + it.program : '' }}</div>
                      <div class="flex items-center gap-3">
                        <button @click.stop="downloadItem(it)" class="text-xs text-slate-300 hover:text-white underline">Скачать</button>
                      </div>
                    </div>
                  </figcaption>
                </figure>
              </template>
            </div>

            <div v-if="items.length === 0" class="p-8 text-center text-slate-400">Нет изображений.</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Preview modal -->
    <transition name="fade">
      <div v-if="preview" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-black/70" @click="closePreview"></div>
        <div class="relative max-w-4xl w-full bg-slate-900 rounded-lg shadow-2xl overflow-hidden border border-white/6">
          <div class="flex items-center justify-between p-3 border-b border-white/6">
            <div class="text-sm text-slate-100">{{ preview.caption || preview.obs }}</div>
            <div class="flex items-center gap-2">
              <button @click="downloadItem(preview)" class="text-sm py-1 px-3 bg-slate-800 rounded border text-slate-100">Скачать</button>
              <button @click="closePreview" class="text-sm py-1 px-3 bg-white/5 rounded border">Закрыть</button>
            </div>
          </div>

          <div class="p-4">
            <img :src="preview.url" :alt="preview.caption || preview.obs" class="w-full h-[70vh] object-contain bg-black rounded" />
            <div class="mt-3 text-sm text-slate-300">
              <div v-if="preview.inst && preview.inst.length">Инструменты: {{ preview.inst.join(', ') }}</div>
              <div v-if="preview.suffix">Суффикс: {{ preview.suffix }}</div>
              <div v-if="preview.program">Программа: {{ preview.program }}</div>
              <div v-if="preview.caption" class="mt-2 text-slate-200">{{ preview.caption }}</div>
            </div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { getJwstFeed, type JwstItem, type JwstFeedResponse, type JwstFeedParams } from '@/api/jwst';

const feed = ref<JwstFeedResponse>({});
const items = ref<JwstItem[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);
const preview = ref<JwstItem | null>(null);

const form = reactive({
  source: 'jpg',
  suffix: '',
  program: '',
  instrument: '',
  perPage: 24,
} as JwstFeedParams);

async function loadFeed(params?: JwstFeedParams) {
  loading.value = true;
  error.value = null;
  try {
    const p: JwstFeedParams = { ...(params ?? {}) };
    const resp = await getJwstFeed(p);
    feed.value = resp ?? {};
    items.value = (resp?.items ?? []).slice(0, Number(resp?.count ?? resp?.items?.length ?? 0));
  } catch (e: any) {
    console.error(e);
    error.value = e?.message ?? 'Ошибка загрузки';
    feed.value = {};
    items.value = [];
  } finally {
    loading.value = false;
  }
}

function onSubmit() {
  const params: JwstFeedParams = { source: form.source, perPage: form.perPage };
  if (form.source === 'suffix' && form.suffix) params.suffix = form.suffix;
  if (form.source === 'program' && form.program) params.program = form.program;
  if (form.instrument) params.instrument = form.instrument;
  loadFeed(params);
}

function openInNewTab(it: JwstItem) {
  const url = it.link || it.url;
  window.open(url, '_blank', 'noopener');
}

function downloadItem(it: JwstItem) {
  try {
    const url = it.url;
    const a = document.createElement('a');
    a.href = url;
    const filename = (it.obs || url.split('/').pop() || 'image.jpg').toString();
    a.setAttribute('download', filename);
    a.setAttribute('rel', 'noopener');
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    a.remove();
  } catch (e) {
    openInNewTab(it);
  }
}

function openPreview(it: JwstItem) {
  preview.value = it;
}

function closePreview() {
  preview.value = null;
}

onMounted(() => {
  loadFeed({ source: form.source, perPage: form.perPage });
});
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-clamp: 2;
  overflow: hidden;
}
.fade-enter-active, .fade-leave-active { transition: opacity .2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>