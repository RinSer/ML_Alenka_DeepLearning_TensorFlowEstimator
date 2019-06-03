import BasicLayout from './BasicLayout';

export default class Train extends BasicLayout {

    async componentDidMount() {
        try {
            let response = await fetch('/api/alenka/train');
            let data = await response.json();
            this.setState({ data, folder: 'inputs' });
        } catch (e) {
            console.error(e.message);
        }
    }

}