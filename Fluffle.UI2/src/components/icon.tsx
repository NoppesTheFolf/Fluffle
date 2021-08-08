import * as React from 'react'
import Svg from '../services/svg'

const Icon = ({ name }: { name: string }) => {
    return (
        <span className="flex" dangerouslySetInnerHTML={{ __html: Svg.getRaw(name) }} />
    )
}

export default Icon
