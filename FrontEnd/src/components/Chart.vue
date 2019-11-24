<script>
import { Line, mixins } from 'vue-chartjs'

export default {
    name: 'Chart',
    extends: Line,
    mixins: [mixins.reactiveData],
    props: ['chartData', 'options'],
    mounted () {
        this.renderChart(this.chartData, this.options)
    }
}
</script>
