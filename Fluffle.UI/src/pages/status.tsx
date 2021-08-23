import * as React from 'react'
import { Helmet } from 'react-helmet'
import Api from '../services/api'
import Layout from '../components/layout'
import Loader from '../components/loader'
import ProgressBar from '../components/progress-bar'
import ProgressBarPart from '../components/progress-bar-part'
import { Chart, TimeScale, LinearScale, LineController, PointElement, LineElement, Tooltip } from 'chart.js'
import 'chartjs-adapter-date-fns'
import Icon from '../components/icon'
const variables = require('../variables')

Chart.register([
    TimeScale,
    LinearScale,
    LineController,
    PointElement,
    LineElement,
    Tooltip
]);

const Status = ({ status }) => {

    function drawChart(canvas: HTMLCanvasElement) {
        if (canvas == null) return;

        const ctx = canvas.getContext('2d');
        const source = status.historyLast30Days;
        const time = 'day';

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: source.map(m => m.when),
                datasets: [
                    {
                        data: source.map(m => m.scrapedCount),
                        borderColor: variables.colors.info.light,
                        borderWidth: 1
                    },
                    {
                        data: source.map(m => m.indexedCount),
                        borderColor: variables.colors.primary.light,
                        borderWidth: 1
                    },
                    {
                        data: source.map(m => m.errorCount),
                        borderColor: variables.colors.danger.light,
                        borderWidth: 1
                    }
                ]
            },
            options: {
                animation: {
                    duration: 0
                },
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        type: 'time',
                        adapters: {
                            date: {
                            }
                        },
                        time: {
                            unit: time
                        },

                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            precision: 0
                        }
                    }
                },
                elements: {
                    point: {
                        radius: 0,
                        hitRadius: 8
                    }
                }
            }
        });
    }

    return (
        <div className="flex flex-wrap justify-center items-center space-x-0 md:space-x-6 space-y-6 w-full max-w-4xl bg-dark-300 rounded p-3 lg:p-8">
            <div className="flex-grow flex flex-col justify-center items-center space-y-6">
                <div className="flex flex-wrap justify-center items-center space-x-6 space-y-6">
                    <div className="w-32 fill-light-100">
                        <Icon name={status.name} />
                    </div>
                    <div className="sm:flex-grow flex flex-col space-y-2 sm:whitespace-nowrap text-sm lg:text-base">
                        {!status.isComplete &&
                            <span>Estimated number of images: {status.estimatedCount.toLocaleString()}</span>
                        }
                        <span>
                            {!status.isComplete && <span>Of which have been scraped:</span>}
                            {status.isComplete && <span>Number of images:</span>}
                            <span> {status.storedCount.toLocaleString()}</span>
                        </span>
                        <span>Of which have been indexed: {status.indexedCount.toLocaleString()}</span>
                    </div>
                </div>
                <div className="w-64 sm:w-full">
                    <ProgressBar>
                        <ProgressBarPart color="bg-info" percentage={status.scrapedPercentage} />
                        <ProgressBarPart color="bg-primary" percentage={status.indexedPercentage} />
                    </ProgressBar>
                </div>
            </div>
            <div className="w-64 lg:w-80">
                <canvas ref={element => drawChart(element)}></canvas>
            </div>
        </div>
    )
}

const StatusPage = () => {
    const [status, setStatus] = React.useState(null);

    React.useEffect(() => {
        Api.status().then(setStatus);
    }, []);

    return (
        <Layout center={true} title="Status">
            <Helmet>
                <meta name="description" content="Information about the indexing progress Fluffle has made."></meta>
            </Helmet>
            {status == null &&
                <div className="w-full flex justify-center items-center">
                    <Loader />
                </div>
            }
            {status != null &&
                <div className="flex flex-wrap items-center space-y-3 justify-center">
                    {status.map(status =>
                        <Status key={status.name} status={status} />
                    )}
                </div>
            }
        </Layout>
    )
}

export default StatusPage
