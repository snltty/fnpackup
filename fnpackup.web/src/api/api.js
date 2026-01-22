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

export const fetchSystemVersion = () => {
    return fetchApi('/system/version',{
        method:'GET',
        headers:{'Content-Type':'application/json'}
    }).then(c=>c.text());
}

export const fetchProjectCreate = (data) => { 
    return fetchApi(`/project/create`,{
        method:'POST',
        headers:{'Content-Type':'application/json'},
        body:JSON.stringify(data)
    }).then(res => res.text());
}
export const fetchProjectExists = (name) => { 
    return fetchApi(`/project/exists`,{
        params:{name:name},
        method:'GET',
        headers:{'Content-Type':'application/json'},
    }).then(res => res.json());
}
export const fetchProjectPack = (name,platform,server) => { 
    return fetchApi(`/project/pack`,{
        params:{name:name,platform:platform,server:server},
        method:'POST',
        headers:{'Content-Type':'application/json'},
    }).then(res => res.json());
}
export const fetchProjectBuild = (name,shell) => { 
    return fetchApi(`/project/build`,{
        params:{name:name,shell:shell},
        method:'POST',
        headers:{'Content-Type':'application/json'},
    }).then(res=>res.text());
}


export const fetchFileList = (path,p,ps) => { 
    return fetchApi(`/file/list`,{
        params:{
            path:path,
            p:p,
            ps:ps
        },
        method:'GET',
        headers:{'Content-Type':'application/json'},
    }).then(res=>res.json());
}
export const fetchFileCreate = (path,f)=>{
    return fetchApi(`/file/create`,{
        params:{path:path,f:f},
        method:'POST',
        headers:{'Content-Type':'application/json'},
    }).then(res=>res.text());
}
export const fetchFileRename = (path,path1,f) => { 
    return fetchApi(`/file/rename`,{
        params:{
            path:path,
            path1:path1,
            f:f
        },
        method:'POST',
        headers:{'Content-Type':'application/json'},
    }).then(res=>res.text());
}
export const fetchFileDelete = (path,f) => { 
    return fetchApi(`/file/delete`,{
        params:{
            path:path,
            f:f
        },
        method:'POST',
        headers:{'Content-Type':'application/json'},
    }).then(res=>res.text());
}
export const fetchFileRead = (path) => {
    return fetchApi(`/file/read`,{
        params:{path:path},
        method:'GET',
        headers:{'Content-Type':'application/json'},
    }).then(res => res.text());
}
export const fetchFileWrite = (path,content) => { 
    return fetchApi(`/file/write`,{
        method:'POST',
        headers:{'Content-Type':'application/json'},
        body:JSON.stringify({
            path:path,
            content:content
        })
    }).then(c=>c.text());
}
export const fetchFileUpload = (path,fpk,formData,progressFn) => { 
    return xhrApi(`/file/upload`,{path:path,fpk:fpk},formData,progressFn);
}


export const fetchPlatform = (name,platform) => { 
    return fetchApi(`/platform/empty`,{
        params:{name:name,platform:platform},
        method:'POST',
        headers:{'Content-Type':'application/json'}
    }).then(c=>c.text());
}


export const fetchAppCenter = (name,names) => { 
    return fetchApi('/app/list',{
        params:{name:name || '',names:names || ''},
        method:'GET',
        headers:{'Content-Type':'application/json'},
    }).then(res => res.json());
}


export const fetchStaticList = () => { 
    return fetchApi('/static/list',{
        method:'GET',
        headers:{'Content-Type':'application/json'},
    }).then(res => res.json());
}
export const fetchStaticSearch = () => { 
    return fetchApi('/static/search',{
        method:'POST',
        headers:{'Content-Type':'application/json'},
    });
}



export const fetchLoggerList = (text,p = 1,ps = 10,type = 0) => { 
    return fetchApi('/logger/list',{
        params:{text:text || '',p:p,ps:ps,type:type},
        method:'GET',
        headers:{'Content-Type':'application/json'},
    }).then(res => res.json());
}