import * as React from 'react'
import { Helmet } from 'react-helmet'
import classNames from 'classnames'
import Navbar from './navbar'
import NavbarMobile from './navbar-mobile'

const Layout = ({ center, title, children }) => {
    return (
        <div className="w-full min-h-full flex flex-col">
            <Helmet>
                <title>{title} - Fluffle</title>
                <link rel="preconnect" href="https://fonts.googleapis.com" />
                <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
                <link href="https://fonts.googleapis.com/icon?family=Material+Icons+Outlined" rel="stylesheet" />
            </Helmet>
            <div className="flex-grow container px-3 pb-3 mx-auto flex flex-col items-center justify-center">
                <Navbar></Navbar>
                <main className={classNames("overflow-hidden sm:max-w-7xl w-full pt-3 flex-grow flex flex-col items-center", { "justify-center": center })} >
                    {children}
                </main>
            </div>
            <NavbarMobile isDummy={true}></NavbarMobile>
            <NavbarMobile isDummy={false}></NavbarMobile>
        </div>
    )
}

export default Layout