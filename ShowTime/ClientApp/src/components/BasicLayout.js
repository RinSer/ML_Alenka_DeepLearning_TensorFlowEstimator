import React from 'react';

export default class BasicLayout extends React.Component {
    _consoleStyle = {
        marginTop: '50px'
    };
    _logElements = [<div key="initial_message"><p className="console">Процедура обучения модели</p></div>];
    _consoleHTML = (<div style={this._consoleStyle}>{this._logElements}</div>);
    _lastLog = "";
    _isUpdating = false;

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

        return this._consoleHTML;
        
    }

    componentDidUpdate(prevProps) {
        if (this.props.logging !== prevProps.logging) {
            console.log(this.props.logging);
            let newElement =
                (<div key={this.props.logging} dangerouslySetInnerHTML={{ __html: this.props.logging }}></div>);
            this._logElements.push(newElement)
            console.log(this._logElements);
        }
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