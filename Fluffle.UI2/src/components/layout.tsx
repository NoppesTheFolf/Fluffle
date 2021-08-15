import * as React from 'react'
import { Helmet } from 'react-helmet'
import classNames from 'classnames'
import Navbar from './navbar'
import NavbarMobile from './navbar-mobile'
import { MDXProvider } from "@mdx-js/react"

export const LayoutWidth = {
    '2xl': 'sm:max-w-2xl',
    '4xl': 'sm:max-w-4xl',
    '7xl': 'sm:max-w-7xl'
}

const Layout = ({ center, title, maxWidth, requireBrowser, children }) => {
    const [hasBrowser, setHasBrowser] = React.useState(false);

    const navbarRef: React.RefObject<HTMLElement> = React.useRef();
    const [dummyHeight, setDummyHeight] = React.useState(0);

    function onResize() {
        setDummyHeight(navbarRef.current?.clientHeight ?? 0);
    }

    React.useEffect(() => {
        setHasBrowser(true);
        onResize();
        window.addEventListener('resize', onResize);

        return () => {
            window.removeEventListener('resize', onResize);
        };
    }, [])

    return (
        <div className="w-full min-h-full flex flex-col">
            <Helmet>
                <title>{title} - Fluffle</title>
            </Helmet>
            <div className="flex-grow container px-3 pb-3 mx-auto flex flex-col items-center justify-center">
                <Navbar />
                <main className={classNames(`overflow-hidden ${LayoutWidth[maxWidth]} w-full pt-3 flex-grow flex flex-col items-center`, { "justify-center": center })} >
                    {(!requireBrowser || (requireBrowser && hasBrowser)) &&
                        <div className="w-full">
                            {children}
                        </div>
                    }
                </main>
                <div style={{ height: dummyHeight }}></div>
            </div>
            <NavbarMobile ref={navbarRef} />
        </div>
    )
}

Layout.defaultProps = {
    maxWidth: '7xl',
    requireBrowser: false,
};

export default Layout
