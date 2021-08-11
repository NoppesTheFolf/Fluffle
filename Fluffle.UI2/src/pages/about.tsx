import { Link } from 'gatsby'
import * as React from 'react'
import Layout from '../components/layout'
import Banner from '../components/banner'

const AboutPage = () => {
    return (
        <Layout center={true} title="About" maxWidth="2xl">
            <div className="my-12 text-center">
                <Banner></Banner>
            </div>
            <div className="prose max-w-none">
                <p>
                    Fluffle is an initiative taken by me, <Link to="/contact/" >Noppes</Link>, with the goal to eventually create a single
                    unified platform for fellow furries with which they can reverse search artwork from a variety of sources. In its early
                    stages Fluffle will remain closed source, but I might decide to make it public domain under the GPLv3 (The GNU General
                    Public License v3.0) license in the future.
                </p>
                <a id="privacy">
                    <h2>Privacy</h2>
                </a>
                <p>
                    When you reverse search an image on Fluffle, the image gets stored temporarily on our servers for processing.
                    Processing only contains fingerprinting the image. That is creating an identifier which can be used to compare
                    indexed images. Once the fingerprint is created, the submitted image gets permanently deleted immediately. The
                    aforementioned fingerprint also doesn’t get stored and neither do the settings used for reverse searching.
                </p>
                <p>
                    Said settings are stored in your browser its so-called local storage. You can
                    think of this as a place in which websites can store a small amount of data. These preferences are only
                    sent to Fluffle whenever you submit an image for reverse searching, and as said before, are immediately
                    discarded when they are not required anymore for fulfilling your request.
                </p>
                <p>
                    However, we do log a few things regarding your request. These are strictly used for assessing server performance and request throttling/blocking. The following information is being logged:
                </p>
                <ul>
                    <li>The origin of the request, needed to identify malicious clients and block them.</li>
                    <li>User Agent, for a normal user, this would be the web browser and the version of said web browser. Also used to identify malicious clients. Mainly used to identify which applications are making use of Fluffle its API.</li>
                    <li>If your request caused the server to error out for whatever reason, the corresponding stack trace of the occurred exception gets logged. We can use this information to determine what caused your request to fail if you decide to contact us and provide us with a trace ID, the used image and the settings used.</li>
                    <li>The format in which the image is encoded and its width and height. These three image attributes correlate highly with the time needed to process your request. It is therefore used to assess server performance. Its also used to identify API clients that don’t follow the guidelines put up by Fluffle.</li>
                    <li>The moment at which the server started processing your request. This can be used to assess server performance in scenarios where there are a lot of request being processed in parallel.</li>
                    <li>The how manyth request this is since the start of the server. Used to assess server performance.</li>
                    <li>A bunch of timings. You can compare this to having a stopwatch and tracking laps, but in application code. How long did it take to create the fingerprint? How long did it take to check if the submitted file was an image? How long did it take to compare the fingerprint? Etc. Used to assess server performance.</li>
                    <li>The number of images compared against. Used to assess server performance.</li>
                </ul>
                <a id="privacy">
                    <h2>Tracking & cookies</h2>
                </a>
                <p>
                    Fluffle uses <a href="https://www.cloudflare.com/web-analytics/" target="_blank" rel="noreferrer">Cloudflare its web analytics platform</a>.
                    They do not collect personal information and also do not track your browsing behavior. They do not make use of cookies and neither do we, meaning Fluffle is cookie free.
                </p>
            </div>
        </Layout>
    )
}

export default AboutPage
