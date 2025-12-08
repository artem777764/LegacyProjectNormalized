<template>
  <div class="min-h-screen p-8 bg-gradient-to-b from-slate-950 via-slate-900 to-slate-800 text-white">
    <div class="max-w-[1200px] mx-auto">
      <div class="flex items-start justify-between gap-4 mb-6">
        <div>
          <h1 class="text-4xl font-extrabold tracking-tight mb-1">NASA OSDR</h1>
          <p class="text-sm text-slate-300 max-w-2xl">
            Таблица OSDR-датасетов. Здесь показан список доступных наборов данных с ссылками на REST-энды и
            возможностью просмотреть "raw" JSON для каждой строки.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <button
            @click="reload"
            class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-sky-600/90 hover:bg-sky-500 transition shadow"
          >
            Обновить
          </button>
        </div>
      </div>

      <div class="mb-4 flex items-center justify-between gap-4">
        <div class="text-sm text-slate-400">
          Всего: <span class="font-medium text-slate-200">{{ total }}</span>
        </div>
        <div class="text-sm text-slate-400">
          Источник: <span class="font-mono text-sky-300">/Osdr/list/all</span>
        </div>
      </div>

      <OsdrTable ref="tableRef" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import OsdrTable from '@/components/OsdrTable.vue';

const tableRef = ref<InstanceType<typeof OsdrTable> | null>(null);

const total = computed(() => {
  // @ts-ignore
  const items = tableRef.value?.items ?? null;
  return items ? items.length : '—';
});

function reload() {
  // @ts-ignore
  if (tableRef.value?.refresh) {
    // @ts-ignore
    tableRef.value.refresh();
    return;
  }
  window.location.reload();
}
</script>

<style scoped>

</style>