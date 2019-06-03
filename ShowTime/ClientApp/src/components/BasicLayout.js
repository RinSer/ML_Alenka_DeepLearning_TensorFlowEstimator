import React from 'react';

export default class BasicLayout extends React.Component {

    render() {

        if (!(this.state && this.state.data)) {
            return <h1>Данные обрабатываются...</h1>
        }

        let imgStyle = { float: 'right' };
        let liStyle = {
            height: '105px',
            borderBottom: '2px solid grey'
        };

        let data = this.state.data.map(item =>
            <li style={liStyle} key={item.imagePath}>
                <img width="100" height="100"
                    style={imgStyle}
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