<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { endpoints } from '@/api/endpoints';
import type { AstroResponse, AstroRow } from '@/models/AstroEvent';

const astroData = ref<AstroResponse | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);
const openedRow = ref<string | null>(null);

const form = reactive({
  lat: 55.7558,
  lon: 37.6176,
  days: 7,
  keyword: ''
});

const sortKey = ref<'name' | 'cells' | null>('name');
const sortDir = ref<'asc' | 'desc'>('asc');

async function loadAstro() {
  loading.value = true;
  error.value = null;
  try {
    astroData.value = await endpoints.getAstroEvents(form.lat, form.lon, form.days);
  } catch (e: any) {
    console.error(e);
    error.value = e?.message ?? 'Ошибка загрузки';
  } finally {
    loading.value = false;
  }
}

function formatEvents(cells: AstroRow['cells']): string {
  if (!cells || !cells.length) return '—';
  return cells
    .map((c: any) => {
      if (Array.isArray(c)) {
        return c.map(e => e.type || '—').join(', ');
      } else if (c.type) {
        return c.type;
      }
      return '—';
    })
    .join(', ');
}

onMounted(loadAstro);

const filteredRows = computed(() => {
  if (!astroData.value) return [];
  let rows = [...astroData.value.data.table.rows];

  if (form.keyword.trim()) {
    const kw = form.keyword.trim().toLowerCase();
    rows = rows.filter(r =>
      r.entry.name.toLowerCase().includes(kw) ||
      r.cells.some(c => JSON.stringify(c).toLowerCase().includes(kw))
    );
  }

  if (sortKey.value) {
    const key = sortKey.value;
    const dir = sortDir.value === 'asc' ? 1 : -1;
    rows.sort((a, b) => {
      let av: any, bv: any;
      if (key === 'name') {
        av = a.entry.name; bv = b.entry.name;
      } else {
        av = JSON.stringify(a.cells); bv = JSON.stringify(b.cells);
      }
      av = av ?? ''; bv = bv ?? '';
      if (typeof av === 'string' && typeof bv === 'string') {
        return av.localeCompare(bv) * dir;
      }
      return 0;
    });
  }

  return rows;
});

function toggleSort(key: typeof sortKey.value) {
  if (sortKey.value === key) sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
  else {
    sortKey.value = key;
    sortDir.value = 'asc';
  }
}

function sortIcon(key: typeof sortKey.value) {
  if (sortKey.value !== key) return 'opacity-30';
  return sortDir.value === 'asc' ? 'rotate-180' : '';
}
</script>

<template>
  <div class="p-6 max-w-7xl mx-auto">
    <div class="bg-white/3 border border-white/6 rounded-2xl shadow-xl p-4">
      <h2 class="text-lg font-semibold mb-4 text-slate-100">Астрономические события (AstronomyAPI)</h2>

      <form @submit.prevent="loadAstro" class="flex flex-wrap gap-2 items-center mb-4">
        <input v-model.number="form.lat" type="number" step="0.0001" placeholder="Широта" class="input-sm" />
        <input v-model.number="form.lon" type="number" step="0.0001" placeholder="Долгота" class="input-sm" />
        <input v-model.number="form.days" type="number" min="1" max="365" placeholder="Дней" class="input-sm w-20" />
        <input v-model="form.keyword" type="text" placeholder="Ключевое слово" class="input-sm flex-1" />
        <button type="submit" class="btn-sm btn-primary">Показать</button>
      </form>

      <div v-if="loading" class="text-center py-8 text-gray-300">Загрузка…</div>
      <div v-if="error" class="text-red-400 py-4">{{ error }}</div>

      <div v-if="!loading && astroData" class="overflow-x-auto rounded-2xl border border-white/10 backdrop-blur bg-white/5 shadow-xl">
        <table class="w-full text-sm text-center">
          <thead class="bg-white/10 text-gray-300 uppercase text-xs tracking-wider">
            <tr>
              <th class="px-4 py-3 cursor-pointer" @click="toggleSort('name')">
                # <span :class="['inline-block transition', sortIcon('name')]">▼</span>
              </th>
              <th class="px-4 py-3 cursor-pointer" @click="toggleSort('name')">
                Тело <span :class="['inline-block transition', sortIcon('name')]">▼</span>
              </th>
              <th class="px-4 py-3 cursor-pointer" @click="toggleSort('cells')">
                События <span :class="['inline-block transition', sortIcon('cells')]">▼</span>
              </th>
            </tr>
          </thead>
          <tbody>
            <template v-for="(row, index) in filteredRows" :key="row.entry.id">
              <tr class="hover:bg-white/10 cursor-pointer transition" @click="openedRow = openedRow === row.entry.id ? null : row.entry.id">
                <td class="px-4 py-2 text-gray-400 align-middle">{{ index + 1 }}</td>
                <td class="px-4 py-2 text-sky-400 font-mono align-middle">{{ row.entry.name }}</td>
                <td class="px-4 py-2 truncate align-middle">{{ formatEvents(row.cells) }}</td>
              </tr>
              <tr>
                <td colspan="3" class="p-0">
                  <div class="overflow-hidden transition-all duration-300" :class="openedRow === row.entry.id ? 'max-h-96 p-2' : 'max-h-0 p-0'">
                    <pre class="bg-black/40 text-green-300 text-xs p-2 rounded">{{ JSON.stringify(row, null, 2) }}</pre>
                  </div>
                </td>
              </tr>
            </template>

            <tr v-if="!filteredRows.length">
              <td colspan="3" class="text-center py-10 text-gray-500">Нет данных</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.input-sm { @apply text-sm py-1 px-2 rounded border bg-slate-800 text-slate-100; }
.btn-sm { @apply text-sm py-1 px-3 rounded bg-indigo-600 hover:bg-indigo-500 text-white; }
thead span { display: inline-block; transform-origin: center; }
</style>