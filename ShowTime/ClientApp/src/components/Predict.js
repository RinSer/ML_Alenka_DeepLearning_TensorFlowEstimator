import BasicLayout from './BasicLayout';

export default class Predict extends BasicLayout {

    async componentDidMount() {
        try {
            let response = await fetch('/api/alenka/predict');
            let data = await response.json();
            this.setState({ data, folder: 'outputs' });
        } catch (e) {
            console.error(e.message);
        }
    }

}