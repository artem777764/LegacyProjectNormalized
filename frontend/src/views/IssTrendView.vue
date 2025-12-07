<template>
  <div class="min-h-screen bg-gradient-to-b from-slate-950 via-slate-900 to-slate-800 text-slate-100 pt-20">
    <div class="max-w-7xl mx-auto px-4 py-8">
      <div class="flex flex-col md:flex-row gap-6">
        <!-- main -->
        <section class="flex-1">
          <div class="bg-white/3 rounded-2xl shadow-lg overflow-hidden border border-white/6 backdrop-blur-md">
            <div class="px-6 py-4 border-b border-white/6 flex items-center justify-between">
              <h2 class="text-lg font-semibold text-slate-100">МКС — положение и движение</h2>
              <div class="text-sm text-slate-300">
                Обновлено: <span class="font-medium text-indigo-200">{{ lastFetchText }}</span>
              </div>
            </div>

            <div class="p-6">
              <div ref="mapEl" class="w-full rounded-lg border bg-slate-900/40 overflow-hidden" style="height:520px"></div>

              <div class="mt-4 grid grid-cols-2 gap-3 md:grid-cols-4">
                <div class="bg-slate-900/40 rounded-lg p-3 border border-white/6 shadow-sm transition transform hover:-translate-y-0.5">
                  <div class="text-xs text-slate-400">Движение</div>
                  <div class="text-lg font-semibold text-white">{{ trend?.movement ? 'Да' : 'Нет' }}</div>
                </div>

                <div class="bg-slate-900/40 rounded-lg p-3 border border-white/6 shadow-sm transition transform hover:-translate-y-0.5">
                  <div class="text-xs text-slate-400">Пройдено (км)</div>
                  <div class="text-lg font-semibold text-white">{{ deltaKmText }}</div>
                </div>

                <div class="bg-slate-900/40 rounded-lg p-3 border border-white/6 shadow-sm transition transform hover:-translate-y-0.5">
                  <div class="text-xs text-slate-400">Скорость (км/ч)</div>
                  <div class="text-lg font-semibold text-white">{{ velocityText }}</div>
                </div>

                <div class="bg-slate-900/40 rounded-lg p-3 border border-white/6 shadow-sm transition transform hover:-translate-y-0.5">
                  <div class="text-xs text-slate-400">Длительность</div>
                  <div class="text-lg font-semibold text-white">{{ durationText }}</div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- sidebar -->
        <aside class="w-full md:w-96">
          <div class="sticky top-28 space-y-4">
            <div class="bg-white/3 rounded-2xl shadow-lg p-6 border border-white/6">
              <h3 class="text-sm font-semibold text-slate-100 mb-3">Информация</h3>
              <dl class="grid grid-cols-1 gap-y-3 text-sm text-slate-200">
                <div class="flex justify-between">
                  <dt class="text-xs text-slate-400">От</dt>
                  <dd class="text-sm text-slate-100">{{ formattedFromTime }}</dd>
                </div>

                <div class="flex justify-between">
                  <dt class="text-xs text-slate-400">До</dt>
                  <dd class="text-sm text-slate-100">{{ formattedToTime }}</dd>
                </div>

                <div class="flex justify-between">
                  <dt class="text-xs text-slate-400">От (lat,lon)</dt>
                  <dd class="text-sm text-slate-100">{{ fromLatLon }}</dd>
                </div>

                <div class="flex justify-between">
                  <dt class="text-xs text-slate-400">До (lat,lon)</dt>
                  <dd class="text-sm text-slate-100">{{ toLatLon }}</dd>
                </div>
              </dl>
            </div>

            <div class="bg-white/3 rounded-2xl shadow-lg p-6 border border-white/6">
              <h3 class="text-sm font-semibold text-slate-100 mb-3">Ручные действия</h3>
              <div class="flex flex-col gap-3">
                <button @click="refresh" class="w-full text-sm py-2 rounded-md bg-indigo-600 hover:bg-indigo-500 text-white transition">
                  Обновить сейчас
                </button>

                <button @click="stopPolling" class="w-full text-sm py-2 rounded-md bg-white/5 hover:bg-white/8 text-rose-400 border border-white/6 transition">
                  Остановить авто-обновление
                </button>

                <button @click="startPolling" class="w-full text-sm py-2 rounded-md bg-white/5 hover:bg-white/8 text-emerald-300 border border-white/6 transition">
                  Включить авто-обновление (10s)
                </button>
              </div>
            </div>

            <div v-if="loading" class="bg-white/5 rounded-lg p-4 text-sm text-slate-200">Загрузка…</div>

            <div v-if="error" class="bg-rose-900/20 border border-rose-700/30 text-rose-200 rounded-lg p-4">
              Ошибка: {{ error }}
            </div>
          </div>
        </aside>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { endpoints } from '@/api/endpoints';
import type { IssTrend } from '@/models/IssTrend';
import { formatToMoscowTime } from '@/utils/datetime';

const trend = ref<IssTrend | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);
const lastFetchAt = ref<Date | null>(null);

let timer: number | null = null;
const POLL_MS = 10_000;

const mapEl = ref<HTMLDivElement | null>(null);
let L: any = null;
let mapInstance: any = null;
let polyline: any = null;
let markerFrom: any = null;
let markerTo: any = null;

function formatNumber(n: number, digits = 2) {
  return n?.toLocaleString(undefined, { maximumFractionDigits: digits, minimumFractionDigits: digits });
}

function secToHMS(secRaw: number) {
  if (!Number.isFinite(secRaw)) return '-';
  const sec = Math.max(0, Math.round(secRaw));
  const h = Math.floor(sec / 3600);
  const m = Math.floor((sec % 3600) / 60);
  const s = sec % 60;
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}

const lastFetchText = computed(() => lastFetchAt.value ? formatToMoscowTime(lastFetchAt.value.toISOString()) : '—');
const deltaKmText = computed(() => trend.value ? formatNumber(trend.value.deltaKm, 2) : '—');
const velocityText = computed(() => trend.value ? formatNumber(trend.value.velocityKmh, 2) : '—');
const durationText = computed(() => trend.value ? secToHMS(trend.value.dtSec) : '—');

const formattedFromTime = computed(() => trend.value ? formatToMoscowTime(trend.value.fromTime) : '—');
const formattedToTime = computed(() => trend.value ? formatToMoscowTime(trend.value.toTime) : '—');

const fromLatLon = computed(() => trend.value ? `${trend.value.fromLat.toFixed(6)}, ${trend.value.fromLon.toFixed(6)}` : '—');
const toLatLon = computed(() => trend.value ? `${trend.value.toLat.toFixed(6)}, ${trend.value.toLon.toFixed(6)}` : '—');

async function ensureMapLib() {
  if (L) return L;
  const mod = await import('leaflet');
  await import('leaflet/dist/leaflet.css');
  L = (mod as any).default ?? mod;
  return L;
}

async function initMap() {
  if (!mapEl.value) return;
  const lib = await ensureMapLib();
  if (mapInstance) return mapInstance;

  mapInstance = lib.map(mapEl.value, { attributionControl: false }).setView([0, 0], 2);
  lib.tileLayer('https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png', { noWrap: true }).addTo(mapInstance);

  polyline = lib.polyline([], { color: '#60a5fa', weight: 3 }).addTo(mapInstance);
  markerFrom = lib.circleMarker([0, 0], { radius: 6, color: '#34d399', fillColor: '#34d399', fillOpacity: 0.95 }).addTo(mapInstance);
  markerTo = lib.circleMarker([0, 0], { radius: 6, color: '#fb7185', fillColor: '#fb7185', fillOpacity: 0.95 }).addTo(mapInstance);

  return mapInstance;
}

function updateMapWithTrend(t: IssTrend) {
  if (!mapInstance || !L) return;
  const pts: [number, number][] = [
    [t.fromLat, t.fromLon],
    [t.toLat, t.toLon],
  ];
  polyline.setLatLngs(pts);
  markerFrom.setLatLng(pts[0]);
  markerTo.setLatLng(pts[pts.length - 1]);

  const bounds = L.latLngBounds(pts);
  if (bounds.isValid()) {
    mapInstance.fitBounds(bounds.pad(0.2));
  }
}

async function loadTrend() {
  loading.value = true;
  error.value = null;
  try {
    const t = await endpoints.getIssTrend();
    trend.value = t;
    lastFetchAt.value = new Date();
    await initMap();
    updateMapWithTrend(t);
  } catch (e: any) {
    console.error('loadTrend error', e);
    error.value = e?.message ?? String(e);
  } finally {
    loading.value = false;
  }
}

function startPolling() {
  stopPolling();
  timer = window.setInterval(() => {
    loadTrend();
  }, POLL_MS);
}

function stopPolling() {
  if (timer !== null) {
    clearInterval(timer);
    timer = null;
  }
}

function refresh() {
  loadTrend();
}

onMounted(async () => {
  await initMap();
  await loadTrend();
  startPolling();
});

onUnmounted(() => {
  stopPolling();
  if (mapInstance) {
    try { mapInstance.remove(); } catch {}
    mapInstance = null;
  }
});
</script>

<style scoped>
.leaflet-container { width: 100%; height: 100%; }
</style>
