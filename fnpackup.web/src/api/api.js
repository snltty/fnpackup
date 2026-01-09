const BASE_URL = process.env.NODE_ENV === 'development' ? `http://localhost:1069` : '';
export const fetchApi = (url,options = {}) => {
    let uri = BASE_URL + url + '?1=1';
    if(options.params){
        uri += '&' + Object.keys(options.params).map(key => `${key}=${options.params[key]}`).join('&');
        delete options.params;
    }
    return fetch(uri ,options);
}
export const xhrApi = (url,params,data,progressFn) => {
    let uri = BASE_URL + url + '?1=1';
    if(params){
        uri += '&' + Object.keys(params).map(key => `${key}=${params[key]}`).join('&');
    }

    return new Promise((resolve, reject) => { 
        try {
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('progress', (event) => {
                if (event.lengthComputable && progressFn) {
                    if(event.loaded < event.total){
                        const progress = (event.loaded / event.total) * 100;
                        progressFn(progress);
                    }
                }
            });
            xhr.addEventListener('load', () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        resolve(response);
                    } catch (e) {
                        resolve(xhr.responseText);
                    }
                } else {
                    reject(new Error(`上传失败: ${xhr.status}`));
                }
            });
            xhr.addEventListener('error', () => {
                reject(new Error('网络错误'));
            });
            xhr.addEventListener('abort', () => {
                reject(new Error('上传已取消'));
            });
            xhr.open('POST', uri);
            xhr.send(data);
        } catch (error) {
            reject(error);
        }
    });
}

export const fetchRead = (path) => {
    return fetchApi(`/files/read`,{
        params:{path:path},
        method:'GET',
        headers:{'Content-Type':'application/json'},
    });
}
export const fetchWrite = (path,content) => { 
    return fetchApi(`/files/write`,{
        method:'POST',
        headers:{'Content-Type':'application/json'},
        body:JSON.stringify({
            path:path,
            content:content
        })
    });
}
export const fetchDelete = (path,f) => { 
    return fetchApi(`/files/delfile`,{
        params:{ path:path, f:f},
        method:'POST',
        headers:{'Content-Type':'application/json'}
    });
}