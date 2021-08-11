import * as React from 'react'
import { Helmet } from 'react-helmet'
import classNames from 'classnames'
import Navbar from './navbar'
import NavbarMobile from './navbar-mobile'

export const LayoutWidth = {
    '2xl': 'sm:max-w-2xl',
    '7xl': 'sm:max-w-7xl'
}

const Layout = ({ center, title, maxWidth, children }) => {
    return (
        <div className="w-full min-h-full flex flex-col">
            <Helmet>
                <title>{title} - Fluffle</title>
            </Helmet>
            <div className="flex-grow container px-3 pb-3 mx-auto flex flex-col items-center justify-center">
                <Navbar></Navbar>
                <main className={classNames(`overflow-hidden ${LayoutWidth[maxWidth]} w-full pt-3 flex-grow flex flex-col items-center`, { "justify-center": center })} >
                    {children}
                </main>
            </div>
            <NavbarMobile isDummy={true}></NavbarMobile>
            <NavbarMobile isDummy={false}></NavbarMobile>
        </div>
    )
}

Layout.defaultProps = {
    maxWidth: '7xl'
};

export default Layout
