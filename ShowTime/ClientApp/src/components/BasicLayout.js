import React from 'react';

export default class BasicLayout extends React.Component {
    _consoleStyle = {
        marginTop: '25px'
    };

    render() {

        if (this.state && this.state.data && Array.isArray(this.state.data)) {
            const imgStyle = { float: 'right' };
            const liStyle = {
                height: '105px',
                borderBottom: '2px solid grey'
            };

            let data = this.state.data.map(item =>
                <li style={liStyle} key={item.imagePath}>
                    <img width="100" height="100"
                        style={imgStyle} alt={this.translitLabel(item.label)}
                        src={'assets/' + this.state.folder + '/data/' + item.imagePath}
                    />
                    <h3><b>Лейбл:</b> {this.translitLabel(item.label)}</h3>
                    <p><b>Вероятность:</b> {item.probability}</p>
                </li>
            );

            return (
                <div>
                    <ul>{data}</ul>
                </div>
            );
        }

        return (<div style={this._consoleStyle}>
            {this.props.logging.map((log, idx) => <p key={log + idx} className="console">{log}</p>)}
        </div>);
        
    }

    translitLabel(label) {
        switch (label) {
            case 'Alenka':
                return 'Алёнка';
            case 'MISHKA KOSOLAPYI':
                return 'МИШКА КОСОЛАПЫЙ';
            case 'KRASNAYA SHAPOCHKA':
                return 'КРАСНАЯ ШАПОЧКА';
            default:
                return '';
        }
    }

}