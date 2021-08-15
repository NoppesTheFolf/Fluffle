import * as React from 'react'
import svg from '../services/svg'

const Banner = () => {
    return (
        <img className="h-28 inline-block select-none" style={{ maxWidth: "90%" }} src={svg.get("banner")} alt="Fluffle banner" />
    )
}

export default Banner