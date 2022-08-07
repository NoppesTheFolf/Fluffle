import classNames from 'classnames';
import * as React from 'react'
import Svg from '../services/svg'

const Icon = ({ className, name, inheritSize, size }: { className: string | undefined, name: string, inheritSize: boolean, size: string }) => {
    return (
        <span className={classNames("flex", className)} style={{ fontSize: size ? size : inheritSize ? 'inherit' : '1.5em' }} dangerouslySetInnerHTML={{ __html: Svg.getRaw(name) }} />
    )
}

Icon.defaultProps = {
    className: undefined,
    inheritSize: false,
    size: null
};

export default Icon
