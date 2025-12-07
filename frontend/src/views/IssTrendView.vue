<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-7xl mx-auto px-4 py-8">
      <div class="flex flex-col md:flex-row gap-6">

        <section class="flex-1">
          <div class="bg-gray-100 rounded-2xl shadow-md overflow-hidden">
            <div class="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
              <h2 class="text-lg font-semibold text-gray-800">МКС — положение и движение</h2>
              <div class="text-sm text-gray-600">
                Обновлено: <span class="font-medium text-gray-800">{{ lastFetchText }}</span>
              </div>
            </div>

            <div class="p-6">
              <div ref="mapEl" class="w-full rounded-lg border bg-white" style="height: 520px;"></div>

              <div class="mt-4 grid grid-cols-2 gap-3 md:grid-cols-4">
                <div class="bg-white rounded-lg p-3 border shadow-sm">
                  <div class="text-xs text-gray-500">Движение</div>
                  <div class="text-lg font-semibold text-gray-800">{{ trend?.movement ? 'Да' : 'Нет' }}</div>
                </div>

                <div class="bg-white rounded-lg p-3 border shadow-sm">
                  <div class="text-xs text-gray-500">Пройдено (км)</div>
                  <div class="text-lg font-semibold text-gray-800">{{ deltaKmText }}</div>
                </div>

                <div class="bg-white rounded-lg p-3 border shadow-sm">
                  <div class="text-xs text-gray-500">Скорость (км/ч)</div>
                  <div class="text-lg font-semibold text-gray-800">{{ velocityText }}</div>
                </div>

                <div class="bg-white rounded-lg p-3 border shadow-sm">
                  <div class="text-xs text-gray-500">Длительность</div>
                  <div class="text-lg font-semibold text-gray-800">{{ durationText }}</div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <aside class="w-full md:w-96">
          <div class="sticky top-6 space-y-4">
            <div class="bg-white rounded-2xl shadow-md p-6 border">
              <h3 class="text-sm font-semibold text-gray-700 mb-3">Информация</h3>

              <dl class="grid grid-cols-1 gap-y-3">
                <div class="flex justify-between">
                  <dt class="text-xs text-gray-500">От</dt>
                  <dd class="text-sm text-gray-800">{{ formattedFromTime }}</dd>
                </div>

                <div class="flex justify-between">
                  <dt class="text-xs text-gray-500">До</dt>
                  <dd class="text-sm text-gray-800">{{ formattedToTime }}</dd>
                </div>

                <div class="flex justify-between">
                  <dt class="text-xs text-gray-500">От (lat,lon)</dt>
                  <dd class="text-sm text-gray-800">{{ fromLatLon }}</dd>
                </div>

                <div class="flex justify-between">
                  <dt class="text-xs text-gray-500">До (lat,lon)</dt>
                  <dd class="text-sm text-gray-800">{{ toLatLon }}</dd>
                </div>
              </dl>
            </div>

            <div class="bg-white rounded-2xl shadow-md p-6 border">
              <h3 class="text-sm font-semibold text-gray-700 mb-3">Ручные действия</h3>

              <div class="flex flex-col gap-2">
                <button
                  class="w-full text-sm py-2 rounded-md bg-gray-100 hover:bg-gray-200 text-gray-800 border"
                  @click="refresh"
                >
                  Обновить сейчас
                </button>

                <button
                  class="w-full text-sm py-2 rounded-md bg-white hover:bg-gray-50 text-red-600 border"
                  @click="stopPolling"
                >
                  Остановить авто-обновление
                </button>

                <button
                  class="w-full text-sm py-2 rounded-md bg-white hover:bg-gray-50 text-green-600 border"
                  @click="startPolling"
                >
                  Включить авто-обновление (10s)
                </button>
              </div>
            </div>

            <div v-if="error" class="bg-red-50 border border-red-100 text-red-700 rounded-lg p-4">
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
import endpoints from '@/api/endpoints';
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
  L = mod.default ?? mod;
  return L;
}

async function initMap() {
  if (!mapEl.value) return;
  const lib = await ensureMapLib();

  if (mapInstance) return mapInstance;

  mapInstance = lib.map(mapEl.value, { attributionControl: false }).setView([0, 0], 2);
  lib.tileLayer('https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png', { noWrap: true }).addTo(mapInstance);

  polyline = lib.polyline([], { color: '#2563eb', weight: 3 }).addTo(mapInstance);

  markerFrom = lib.circleMarker([0, 0], { radius: 6, color: '#10b981', fillColor: '#10b981', fillOpacity: 0.9 }).addTo(mapInstance);
  markerTo = lib.circleMarker([0, 0], { radius: 6, color: '#ef4444', fillColor: '#ef4444', fillOpacity: 0.9 }).addTo(mapInstance);

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
    try {
      mapInstance.remove();
    } catch {}
    mapInstance = null;
  }
});
</script>

<style scoped>
.leaflet-container {
  width: 100%;
  height: 100%;
}
</style>