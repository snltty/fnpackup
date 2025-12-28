const fs = require('fs');
const yaml = require('js-yaml');
const xml2js = require('xml2js');

const parser = new xml2js.Parser();

function readVersionDesc() {
    return new Promise((resolve, reject) => {
        const fileContents = fs.readFileSync('../../fnpackup/fnpackup.csproj', 'utf8');
        parser.parseString(fileContents, (error, result) => {
            resolve(
                { desc: result.Project.PropertyGroup[0].Description[0], version: result.Project.PropertyGroup[0].FileVersion[0] }
            );
        });
    });
}
function readYaml(path) {
    try {
        const fileContents = fs.readFileSync(path, 'utf8');
        return yaml.load(fileContents);
    } catch (e) {
        console.log(e);
    }
}
function writeYaml(path, data) {
    try {
        const yamlContent = yaml.dump(data);
        return fs.writeFileSync(path, yamlContent, 'utf8');
    } catch (e) {
        console.log(e);
    }
}
function readText(path) {
    try {
        const fileContents = fs.readFileSync(path, 'utf8');
        return fileContents;
    } catch (e) {
        console.log(e);
    }
}
function writeText(path, data) {
    try {
        return fs.writeFileSync(path, data, 'utf8');
    } catch (e) {
        console.log(e);
    }
}
function writeUploadIpk(data, tagName) {
    const types = ['docker'];
    for (let j = 0; j < types.length; j++) { 
        const type = types[j];
        data.jobs.build.steps.push({
            name: `upload-${type}-fpk`,
            id: `upload-${type}-fpk`,
            uses: 'actions/upload-release-asset@master',
            env: {
                'GITHUB_TOKEN': '${{ secrets.ACTIONS_TOKEN }}'
            },
            with: {
                'upload_url': '${{ steps.create_release.outputs.upload_url }}',
                'asset_path': `./public/publish-fpk/${type}/fnpackup-${type}.fpk`,
                'asset_name': `fnpackup-${type}.fpk`,
                'asset_content_type': 'application/fpk'
            }
        });
    }
}


readVersionDesc().then((desc) => {

    let publishText = readText('../ymls/publish-docker.sh');
    while (publishText.indexOf('{{version}}') >= 0) {
        publishText = publishText.replace('{{version}}', desc.version);
    }
    writeText('../publish-docker.sh', publishText);

    let dockerText = readText('../ymls/docker.yml');
    while (dockerText.indexOf('{{version}}') >= 0) {
        dockerText = dockerText.replace('{{version}}', desc.version);
    }
    writeText('../../.github/workflows/docker.yml', dockerText);


    let publishFpkText = readText('../ymls/publish-fpk.sh');
    while (publishFpkText.indexOf('{{version}}') >= 0) {
        publishFpkText = publishFpkText.replace('{{version}}', desc.version);
    }
    writeText('../publish-fpk.sh', publishFpkText);

    let installData = readYaml('../ymls/install.yml');
    installData.jobs.build.steps.filter(c => c.id == 'create_release')[0].with.body = desc.desc;
    installData.jobs.build.steps.filter(c => c.id == 'create_release')[0].with.tag_name = `v${desc.version}`;
    installData.jobs.build.steps.filter(c => c.id == 'create_release')[0].with.release_name = `v${desc.version}.\${{ steps.date.outputs.today }}`;
    writeUploadIpk(installData, `v${desc.version}`);
    writeYaml('../../.github/workflows/install.yml', installData);


});