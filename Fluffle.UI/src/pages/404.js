import * as React from 'react'
import Layout from '../components/layout'

const NotFoundPage = () => {
    // This fixes Cloudflare Pages flashing a 404 screen...
    const [shouldLeaveEmpty, setShouldLeaveEmpty] = React.useState(true);

    React.useEffect(() => {
        setShouldLeaveEmpty(window.location.pathname.startsWith('/mg/'));
    }, []);

    return (
        <Layout center={true} title={shouldLeaveEmpty ? null : "Not found"}>
            {!shouldLeaveEmpty &&
                <div className="flex flex-col items-center text-center">
                    <div className="text-4xl sm:text-7xl font-light mb-4">Blimey! 404 Not Found</div>
                    <div className="text-muted text-xl sm:text-3xl font-light">We couldn't find the page you're looking for</div>
                </div>
            }
        </Layout>
    )
}

export default NotFoundPage
