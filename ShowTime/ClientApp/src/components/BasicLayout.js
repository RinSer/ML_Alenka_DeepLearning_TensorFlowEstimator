import React from 'react';

export default class BasicLayout extends React.Component {
    _consoleStyle = {
        marginTop: '25px'
    };

    toggleLog = () => {
        this.setState({ showLog: !this.state.showLog });
    };

    constructor(props) {
        super(props);
        this.state = { showLog: false };
    }

    render() {

        const log = (<div style={this._consoleStyle}>
            {this.props.logging.map((log, idx) => <p key={idx} className="console">{log}</p>)}
        </div>);
        
        if (this.state && this.state.data && Array.isArray(this.state.data)) {
            const imgStyle = { float: 'right' };
            const liStyle = {
                height: '105px',
                borderBottom: '2px solid grey'
            };

            let data = this.state.data.map(item => {
                let label = "Лейбл:";
                let src = 'assets/' + this.state.folder + '/data/' + item.imagePath;
                if (this.state.folder === 'transfer') {
                    const assets = 'assetss';
                    let path = item.imagePath.split(assets)[1];
                    if (path) {
                        label = "Определённый лейбл:";
                        src = assets + path;
                    } else src = `assetss/inputs-train/data/${item.imagePath}`;
                }
                return (<li style={liStyle} key={item.imagePath}>
                    <img width="100" height="100"
                        style={imgStyle} alt={this.translitLabel(item.label)}
                        src={src}
                    />
                    <h3><b>{label}</b> {this.translitLabel(item.label)}</h3>
                    <p><b>Вероятность:</b> {item.probability}</p>
                </li>);
            });

            if (this.state.showLog) {
                return (<div>
                    <button className="btn btn-submit"
                        onClick={this.toggleLog}>К результатам</button>
                    {log}
                </div>);
            } else {
                return (<div>
                    <button className="btn btn-submit"
                        onClick={this.toggleLog}>К логу</button>
                    <div>
                        <ul>{data}</ul>
                    </div>
                </div>);
            }

            
        }

        return log;
        
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