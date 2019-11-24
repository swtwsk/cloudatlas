<template>
<b-col id="attributes">
    <b-row v-for="(attr, idx) in attributes" :key="idx">
        <b-col>
            <dl class="row">
                <dt class="col-sm-3">
                    <b-link v-if="attr.value.isNumeric && 
                                  attr.value.value !== 'NULL'" 
                        v-on:click="clicked[idx] = !clicked[idx]">
                        {{attr.key}}
                    </b-link>
                    <span v-else>{{attr.key}}</span>
                </dt>
                <dd class="col-sm-9">
                    {{attr.value.value}}
                </dd>
            </dl>
            <b-collapse id="collapse-4" v-model="clicked[idx]" class="mt-2">
                <Chart :chartData="chart[idx]"/>
            </b-collapse>
        </b-col>
    </b-row>
</b-col>
</template>

<script>
import Chart from './Chart.vue'

export default {
    name: 'Attributes',
    components: {
        Chart
    },
    props: {
        zmiPath: null,
    },
    data() {
        return {
            attributes: null,
            aggregated: null,
            times: null,
            clicked: null,
            colors: null,
        }
    },
    methods: {
        fetchData: function(path) {
            const axios = require('axios');

            if (typeof(path) !== "string") {
                return;
            }

            axios
                .get('/zmi' + path)
                .then(response => {this.attributes = response.data})
                .catch(err => console.log(err));
        },
        reset: function(length) {
            var emptyArray = [...Array(length)];

            if (this.aggregated !== null && 
                typeof(this.aggregated) === 'undefined' &&
                length === 0)
                this.aggregated = this.aggregated.map(() => []);
            else
                this.aggregated = emptyArray.map(() => []);

            this.times = new Array();
            if (this.clicked !== null)
                this.clicked = Object.keys(this.clicked)
                    .reduce((result, key) => {
                        result[key] = false
                        return result
                    }, {});

            this.colors = emptyArray
                .map(() => this.HSVtoRGB(Math.random(), 0.5, 0.95))
                .map(c => "rgba(" + c.r + ", " + c.g + ", " + c.b + ", 0.1)");
        },
        // implementation from https://stackoverflow.com/a/17243070
        HSVtoRGB: function(h, s, v) {
            var r, g, b, i, f, p, q, t;
            if (arguments.length === 1) {
                s = h.s, v = h.v, h = h.h;
            }
            i = Math.floor(h * 6);
            f = h * 6 - i;
            p = v * (1 - s);
            q = v * (1 - f * s);
            t = v * (1 - (1 - f) * s);
            switch (i % 6) {
                case 0: r = v, g = t, b = p; break;
                case 1: r = q, g = v, b = p; break;
                case 2: r = p, g = v, b = t; break;
                case 3: r = p, g = q, b = v; break;
                case 4: r = t, g = p, b = v; break;
                case 5: r = v, g = p, b = q; break;
            }
            return {
                r: Math.round(r * 255),
                g: Math.round(g * 255),
                b: Math.round(b * 255),
            };
        },
    },
    computed: {
        chart: function() {
            return this.aggregated
                .map((elem, idx) => new Object({
                    labels: this.times,
                    datasets: [new Object({
                        label: this.attributes[idx].key,
                        backgroundColor: this.colors[idx],
                        data: elem
                    })]
                }));
        },
    },
    watch: {
        zmiPath: function(newPath) {
            this.reset(0);
            this.fetchData(newPath);
        },
        attributes: function(newAtts) {
            if (this.aggregated === null ||
                typeof(this.aggregated) === undefined ||
                this.aggregated.length != newAtts.length)
                this.reset(newAtts.length);
            
            var newValues = newAtts.map(x => x.value.value);
            newValues.forEach((element, idx) => {
                this.aggregated[idx] = this.aggregated[idx].concat(element);
            });
            
            var time = new Date();
            var timeString = ('0' + time.getHours()).slice(-2) + ":" +
                             ('0' + time.getMinutes()).slice(-2) + ":" +
                             ('0' + time.getSeconds()).slice(-2);
            this.times = this.times.concat(timeString);
        }
    },
    created() {
        this.reset(0);
        this.clicked = new Object();
    },
    mounted() {
        this.fetchData(this.zmiPath);
        setInterval(function () {
            this.fetchData(this.zmiPath);
        }.bind(this), 10000); 
    },
}
</script>
