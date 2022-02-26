import * as React from 'react'
import Svg from '../services/svg'

const Icon = ({ name, inheritSize, size }: { name: string, inheritSize: boolean, size: string }) => {
    return (
        <span className="flex" style={{ fontSize: size ? size : inheritSize ? 'inherit' : '1.5em' }} dangerouslySetInnerHTML={{ __html: Svg.getRaw(name) }} />
    )
}

Icon.defaultProps = {
    inheritSize: false,
    size: null
};

export default Icon
