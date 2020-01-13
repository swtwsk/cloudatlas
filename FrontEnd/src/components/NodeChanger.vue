<template>
<div class="container align-items-center justify-content-center">
    <div class="row align-items-center justify-content-center">
        <div class="col">
            <b>Change Agent:</b>
        </div>
        <div class="col-9">
            <b-form inline id="nodeChanger">
                <label for="inline-input-host" 
                    class="mb-2 mr-sm-2 mb-sm-0">Host IP:</label>
                <b-input id="inline-input-host" 
                        placeholder="IP" 
                        class="mb-2 mr-sm-2 mb-sm-0"
                        v-model="host"></b-input>

                <label for="inline-input-port" 
                    class="mb-2 mr-sm-2 mb-sm-0">Port:</label>
                <b-input id="inline-input-port" 
                        placeholder="Port" 
                        class="mb-2 mr-sm-2 mb-sm-0"
                        v-model="port"></b-input>

                <b-button class="my-2 float-right" variant="primary" 
                        v-on:click="change">Set new Agent</b-button>
            </b-form>
        </div>
    </div>
</div>
</template>

<script>
export default {
    name: 'NodeChanger',
    data() {
        return {
            host : String,
            port : Number,
        }
    },
    created() {
        this.host = "";
        this.port = null;
    },
    methods: {
        change: function() {
            const axios = require('axios');
            axios
                .post('/agent', {
                    'host': this.host,
                    'port': this.port,
                })
                .then(response => {
                    if (!response.data) {
                        alert('Could not change host/port');
                    }
                    else {
                        alert('Changed host and port of agent');
                    }
                })
        }
    }
}
</script>