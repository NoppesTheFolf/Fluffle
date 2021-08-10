import * as React from 'react'
import Svg from '../services/svg'

const Icon = ({ name, inheritSize }: { name: string, inheritSize: boolean }) => {
    return (
        <span className="flex" style={{ fontSize: inheritSize ? 'inherit' : '24px' }} dangerouslySetInnerHTML={{ __html: Svg.getRaw(name) }} />
    )
}

Icon.defaultProps = {
    inheritSize: false
};

export default Icon
