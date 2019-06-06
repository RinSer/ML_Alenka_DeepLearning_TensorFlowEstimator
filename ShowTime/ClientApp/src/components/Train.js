import BasicLayout from './BasicLayout';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/ConsoleLog';

class Train extends BasicLayout {

    async componentDidMount() {
        try {
            let response = await fetch('/api/alenka/train');
            let data = await response.json();
            this.setState({ data, folder: 'inputs' });
        } catch (e) {
            if (e) console.error(e.message);
        }
    }

}

export default connect(
    state => state.consoleLog,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Train);