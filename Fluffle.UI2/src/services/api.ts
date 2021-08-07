import axios from 'axios';

const Api = function () {
    function url(segment) {
        return `${process.env.API_URL}/v1/${segment}`
    }

    return {
        async status() {
            const response = await axios.get(url('status'));

            return response.data.map(status => {
                status.scrapedPercentage = status.isComplete ? 100 : Math.round(status.storedCount / status.estimatedCount * 100);
                status.indexedPercentage = Math.round(status.indexedCount / (status.isComplete ? status.storedCount : status.estimatedCount) * 100);

                return status;
            });
        }
    }
}();

export default Api
