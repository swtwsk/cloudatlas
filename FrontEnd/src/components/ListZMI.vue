<template>
<b-col id="listZMI">
    <b-list-group v-for="(zmi, index) in list" :key="index">
        <b-list-group-item button 
            :active="index == currIndex"
            v-on:click="clickButton(index, zmi)">
                {{zmi}}
        </b-list-group-item>
    </b-list-group>
</b-col>
</template>

<script>
export default {
    name: 'ListZMI',
    data() {
        return {
            list: null,
            currIndex : Number,
        }
    },
    methods: {
        fetchData: function() {
            const axios = require('axios');
            axios
                .get('/zmis')
                .then(response => {
                    this.list = response.data;
                    if (this.currIndex < 0 && this.list.length > 0) {
                        this.clickButton(0, this.list[0]);
                    }
                })
                .catch(error => {
                    this.list = new Array();
                    this.currIndex = -1;
                    console.log(error);
                })
        },
        clickButton: function(idx, name) {
            this.currIndex = idx;
            this.$emit('clicked', name)
        }
    },
    watch: {
        list: function(newList) {
            this.$emit('length', newList.length);
        }
    },
    mounted() {
        this.fetchData();
        this.currIndex = -1;

        setInterval(function () {
            this.fetchData();
        }.bind(this), 10000); 
    }
}
</script>
