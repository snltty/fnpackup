const BASE_URL = process.env.NODE_ENV === 'development' ? `http://localhost:1069` : '';
export const fetchApi = (url,options = {}) => {
    let uri = BASE_URL + url + '?1=1';
    if(options.params){
        uri += '&' + Object.keys(options.params).map(key => `${key}=${options.params[key]}`).join('&');
        delete options.params;
    }
    return fetch(uri ,options);
}

