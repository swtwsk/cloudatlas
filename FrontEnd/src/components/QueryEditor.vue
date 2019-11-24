<template>
<b-modal id="query-modal" title="Modify queries" size="xl"
         scrollable centered hide-footer v-on:show="reset">
    <div class="card">
        <b-card-header header-tag="nav">
            <b-nav card-header tabs>
                <b-nav-item :active="showInstall"
                            v-on:click="showInstall = true">
                    Install
                </b-nav-item>
                <b-nav-item :active="!showInstall"
                            v-on:click="showInstall = false; fetchQueries()">
                    Uninstall
                </b-nav-item>
            </b-nav>
        </b-card-header>
        <div class="card-body">
            <b-alert v-model="showAlert" variant="danger" 
                     dismissible>
                {{alertText}}
            </b-alert>

            <b-form v-show="showInstall">
                <label for="inline-form-input-name">Name:</label>
                <b-input
                    id="inline-form-input-name"
                    class="mb-2 mx-sm-2 mb-sm-0"
                    placeholder="Name"
                    v-model="newQueryName"
                ></b-input>

                <label for="inline-form-input-queries">Queries:</label>
                <b-form-textarea 
                    id="inline-form-input-queries"
                    class="mb-2 mx-sm-2 mb-sm-0"
                    placeholder="Queries"
                    v-model="newQueries">
                </b-form-textarea>

                <b-button class="my-2 float-right" variant="primary" 
                        v-on:click="install">
                    Install query
                </b-button>
            </b-form>

            <b-form v-show="!showInstall">
                <label for="inline-form-input-name">Query to uninstall:</label>
                <b-form-select v-model="nameToDelete"
                    :options="deleteOptions">
                    <template v-slot:first>
                        <option value="" disabled>
                            -- Please select an option --
                        </option>
                    </template>
                </b-form-select>
                <b-button class="my-2 float-right" variant="danger" 
                        v-show="showDeleteBtn"
                        v-on:click="uninstall">
                    Uninstall query
                </b-button>
            </b-form>
        </div>
    </div>
</b-modal>
</template>

<script>
export default {
    data() {
        return {
            newQueryName : null,
            newQueries : null,
            nameToDelete : String,
            deleteOptions : null,
            showDeleteBtn : Boolean,
            showAlert : Boolean,
            showInstall : Boolean,
            alertText : String,
        }
    },
    created() {
        this.reset();
    },
    watch: {
        showInstall: function() {
            this.showAlert = false;
            this.nameToDelete = "";
            this.showDeleteBtn = false;
        },
        nameToDelete: function(name) {
            this.showDeleteBtn = name !== "";
        }
    },
    methods: {
        reset: function() {
            this.showInstall = true;
            this.showAlert = false;
            this.showDeleteBtn = false;
            this.deleteOptions = new Array();
            this.nameToDelete = "";
        },
        install: function() {
            const axios = require('axios');
            axios
                .post('/query', {
                    '1': ('&' + this.newQueryName + ': ' + this.newQueries)
                })
                .then(response => 
                {
                    if (!response.data[1]) {
                        this.showAlert = true;
                        this.alertText = 'Couldn\'t add query';
                        return;
                    }

                    this.showAlert = false;
                    alert('Query added successfully');
                    this.$bvModal.hide('query-modal')
                });
        },
        uninstall: function() {
            const axios = require('axios');
            axios
                .delete('/query/' + this.nameToDelete)
                .then(response => {
                    if (!response.data){
                        this.showAlert = true;
                        this.alertText = 'Couldn\'t remove query';
                        return;
                    }

                    this.showAlert = false;
                    alert('Query removed successfully');
                    this.$bvModal.hide('query-modal');
                })
                .catch(err => console.log(err));
        },
        fetchQueries: function() {
            const axios = require('axios');
            axios
                .get('/query')
                .then(response => {
                    this.deleteOptions = response.data;
                })
                .catch(error => console.log(error));
        }
    }
}
</script>