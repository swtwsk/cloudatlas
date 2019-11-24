<template>
    <div id="app">
        <b-container class="my-5">
            <h1 class="text-center display-2">CloudAtlas</h1>
        </b-container>
        <b-container>
            <b-row v-show="length > 0">
                <b-col :cols="3">
                    <b-row>
                        <ListZMI v-on:clicked="zmiPath = $event"
                                 v-on:length="length = $event"/>
                    </b-row>
                    <b-container class="mt-3 text-center">
                        <b-button variant="outline-primary" 
                                  v-b-modal.query-modal>
                            Edit queries
                        </b-button>
                        <query-editor />
                    </b-container>
                </b-col>
                <Attributes :zmiPath="zmiPath"/>
            </b-row>
            <div class="text-center" v-show="length === 0">
                <strong>CloudAtlas client does not respond. Check your connection.</strong>
            </div>
        </b-container>
    </div>
</template>

<script>
import ListZMI from './components/ListZMI.vue'
import Attributes from './components/Attributes.vue'
import QueryEditor from './components/QueryEditor.vue'

export default {
    name: 'app',
    components: {
        ListZMI,
        Attributes,
        QueryEditor
    },
    data() {
        return {
            zmiPath : String,
            length : Number,
        }
    },
    created() {
        this.length = 0;
    }
}
</script>
