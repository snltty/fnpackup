<template>
    <div class="h-100">
        <iframe :src="`${baseUrl}/${$route.query.name}`" frameborder="0" width="100%" height="100%"></iframe>
    </div>
</template>

<script>
import { computed } from 'vue';

export default {
    setup () {

        const baseUrl = computed(()=>process.env.NODE_ENV === 'development' ? `http://localhost:1069` : `${window.location.origin}`);

        return {baseUrl}
    }
}
</script>

<style lang="scss" scoped>

</style>