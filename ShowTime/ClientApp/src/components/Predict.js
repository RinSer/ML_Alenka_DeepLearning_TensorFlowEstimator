import BasicLayout from './BasicLayout';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/ConsoleLog';

class Predict extends BasicLayout {

    async componentDidMount() {
        try {
            let response = await fetch('/api/alenka/predict');
            let data = await response.json();
            if (Array.isArray(data))
                this.setState({ data, folder: 'outputs' });
            else throw data;
        } catch (e) {
            if (e) console.error(e.message);
        }
    }

}

export default connect(
    state => state.consoleLog,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Predict);