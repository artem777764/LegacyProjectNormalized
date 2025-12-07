<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { endpoints } from '@/api/endpoints';
import type { OsdrItem } from '@/models/OsdrItem';
import { formatToMoscowTime } from '@/utils/datetime';

const items = ref<OsdrItem[]>([]);
const openedRow = ref<number | null>(null);

const sortKey = ref<keyof OsdrItem | 'restUrl' | null>(null);
const sortDir = ref<'asc' | 'desc'>('asc');

async function load() {
  const data = await endpoints.getOsdrListAll();
  items.value = data;
}
onMounted(load);

defineExpose({ refresh: load, items });

const toggleRow = (id: number) => {
  openedRow.value = openedRow.value === id ? null : id;
};

const extractRestUrl = (item: OsdrItem) => {
  return item.payload?.REST_URL ?? null;
};

function valueForSort(item: OsdrItem, key: keyof OsdrItem | 'restUrl') {
  if (key === 'restUrl') {
    return extractRestUrl(item) ?? '';
  }
  const v = item[key];
  if (key === 'payload') return '';
  if (v === null || v === undefined) return '';
  if (typeof v === 'number') return v;
  if (typeof v === 'string') {
    const isoDate = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/;
    if (isoDate.test(v)) {
      const t = Date.parse(v);
      if (!Number.isNaN(t)) return t;
    }
    return v.toLowerCase();
  }
  return String(v);
}

const sortedItems = computed(() => {
  const data = [...items.value];
  if (!sortKey.value) return data;

  const key = sortKey.value;
  const dir = sortDir.value === 'asc' ? 1 : -1;

  data.sort((a, b) => {
    const av = valueForSort(a, key);
    const bv = valueForSort(b, key);

    if (typeof av === 'number' && typeof bv === 'number') {
      if (av < bv) return -1 * dir;
      if (av > bv) return 1 * dir;
      return 0;
    }

    const as = String(av);
    const bs = String(bv);
    if (as < bs) return -1 * dir;
    if (as > bs) return 1 * dir;
    return 0;
  });

  return data;
});

const toggleSort = (key: typeof sortKey.value) => {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortKey.value = key;
    sortDir.value = 'asc';
  }
};

const sortIcon = (key: typeof sortKey.value) => {
  if (sortKey.value !== key) return 'opacity-30';
  return sortDir.value === 'asc' ? 'rotate-180' : '';
};
</script>

<template>
  <div class="p-4">
    <!-- Таблица: теперь без своего хедера (он на странице) -->
    <div class="overflow-x-auto rounded-2xl border border-white/10 backdrop-blur bg-white/5 shadow-xl">
      <table class="w-full text-sm">
        <thead class="bg-white/10 text-gray-300 uppercase text-xs tracking-wider">
          <tr>
            <th class="px-4 py-3 cursor-pointer" @click="toggleSort('id')">
              # <span :class="['inline-block transition', sortIcon('id')]">▼</span>
            </th>
            <th class="px-4 py-3 cursor-pointer" @click="toggleSort('datasetId')">
              dataset_id <span :class="['inline-block transition', sortIcon('datasetId')]">▼</span>
            </th>
            <th class="px-4 py-3 cursor-pointer" @click="toggleSort('title')">
              title <span :class="['inline-block transition', sortIcon('title')]">▼</span>
            </th>
            <th class="px-4 py-3 cursor-pointer" @click="toggleSort('restUrl')">
              REST_URL <span :class="['inline-block transition', sortIcon('restUrl')]">▼</span>
            </th>
            <th class="px-4 py-3 cursor-pointer" @click="toggleSort('updatedAt')">
              updated_at <span :class="['inline-block transition', sortIcon('updatedAt')]">▼</span>
            </th>
            <th class="px-4 py-3 cursor-pointer" @click="toggleSort('fetchedAt')">
              inserted_at <span :class="['inline-block transition', sortIcon('fetchedAt')]">▼</span>
            </th>
            <th class="px-4 py-3">raw</th>
          </tr>
        </thead>

        <tbody>
          <template v-for="(row, index) in sortedItems" :key="row.id">
            <tr
              class="transition-all duration-200
                     hover:bg-white/10 hover:scale-[1.002]
                     active:scale-[0.995] cursor-pointer group"
            >
              <td class="px-4 py-3 text-gray-400">
                {{ index + 1 }}
              </td>
              <td class="px-4 py-3 font-mono text-blue-300">
                {{ row.datasetId || '—' }}
              </td>
              <td class="px-4 py-3 max-w-xl truncate text-gray-200">
                {{ row.title || '—' }}
              </td>
              <td class="px-4 py-3">
                <a
                  v-if="extractRestUrl(row)"
                  :href="extractRestUrl(row)!"
                  target="_blank"
                  rel="noopener"
                  class="text-sky-400 hover:text-sky-300 underline underline-offset-2 transition"
                  @click.stop
                >
                  открыть
                </a>
                <span v-else class="text-gray-500">—</span>
              </td>
              <td class="px-4 py-3 text-gray-300">
                {{ formatToMoscowTime(row.updatedAt) }}
              </td>
              <td class="px-4 py-3 text-gray-400">
                {{ formatToMoscowTime(row.fetchedAt) }}
              </td>
              <td class="px-4 py-3">
                <button
                  class="px-3 py-1 rounded-lg border border-white/20
                         text-xs text-gray-200 hover:bg-white/10
                         transition"
                  @click.stop="toggleRow(row.id)"
                >
                  JSON
                </button>
              </td>
            </tr>

            <!-- раскрывающийся JSON -->
            <tr class="transition-all duration-300 ease-in-out">
              <td colspan="7" class="p-0">
                <div
                  class="overflow-hidden transition-all duration-300 ease-in-out"
                  :class="openedRow === row.id ? 'max-h-96 opacity-100' : 'max-h-0 opacity-0'"
                >
                  <pre class="bg-black/40 text-green-300 text-xs p-4 rounded-xl m-2 overflow-auto">
{{ JSON.stringify(row, null, 2) }}
                  </pre>
                </div>
              </td>
            </tr>
          </template>

          <tr v-if="!sortedItems.length">
            <td colspan="7" class="text-center text-gray-500 py-10">
              нет данных
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
thead span {
  display: inline-block;
  transform-origin: center;
}
</style>