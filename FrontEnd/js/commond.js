var headFile;
var urlBase = "https://localhost:44346/";
var valuesSelect;
var csvDocument;

async function loadTable(){

    if(headFile == undefined){
        alert("no hay una ruta de archivo valida")
        return;
    }

    var tableS = $('#txtTable').val();
    var urlTable = urlBase + "GetTableStructure?ConnectionName=MySQLEngeni&TableName=" + tableS //Consulta de la estructura de la tabla
    
    let response = await fetch(urlTable, {
        method: 'GET'
    })
    .then(response => response.json())
    .then(result =>  {
       
            let headItemsFile = headFile.split(";");
            $("#CompareTableStructure tr td").remove(); 
            
            //console.log(result);

            for(let key in result)
            {    
                var TableName = JSON.stringify(result[key]);
                var init = TableName.indexOf(':') + 2;                
                TableName = TableName.substring(init, TableName.length - 2);
                
                addTableimport(TableName, key, headItemsFile)  
                
                
            }  
            
            $('#loadSave').show();
        })
        .catch((error) => {
            alert('No se logró conectar al servicio, error: ' + error);
        });
}

function addTableimport(TableName, key, headItemsFile){

    var newRow = document.createElement("tr");
                
    var newCell = document.createElement("td");
    newCell.innerHTML = TableName;        
    newRow.append(newCell);
    document.getElementById("CompareTableStructureRows").appendChild(newRow); 

    var idSelect = 'SelColFile' + key;

    var newCell = document.createElement("td");
    newCell.innerHTML = '<select id="' + idSelect + '" style="width: 300px; border:none;"><option> -- Seleccione un registro -- </option>';        
    newRow.append(newCell);
    document.getElementById("CompareTableStructureRows").appendChild(newRow);    
    
    //Aquí se cargan los selectBox de cada registro
    for(let itemAdd in headItemsFile)
    {                    
        addSelectOption(idSelect, headItemsFile[itemAdd]);
    }
    
    if(valuesSelect != undefined){
        var nk = parseInt (key) + 1;  
        var vSelect = valuesSelect[nk]    
        console.log('Key: ' + nk + ', valor: ' + vSelect)
    
        var break_ = vSelect.indexOf(',fieldFile:');
        var col1 = vSelect.substring(0,break_);
        var col2 = vSelect.substring(break_ + 11,vSelect.length)
        var break_2 = col2.indexOf('}');
        col2 = col2.substring(0,break_2) == 'vacio' ? '-- Seleccione un registro --' : col2.substring(0,break_2);
    
        $('#' + idSelect).val(col2);
    }
    
}

function addSelectOption(element, optionTxt){
    var select = document.getElementById(element);
    var option = document.createElement("option");
    option.text = optionTxt;
    select.add(option);
}

async function loadFile(){

    let ret;

    var fileS = $('#txtFile').val();
    var urlFile = urlBase + "GetFileStructure?path=" + fileS;
    let response = await fetch(urlFile, {
        method: 'GET'
    })
    .then(response => response.json())
    .then(resultFile =>  {
        for(let keyFile in resultFile)
        {
            var FileItem = JSON.stringify(resultFile[keyFile]);

            var init = FileItem.indexOf(':') + 2;  
            FileItem = FileItem.substring(init,FileItem.length - 2);

            //ret.push(FileItem);
            console.log(FileItem);
        }
    });

    //return ret;

}

function readFile(e) {

    var archivo = e.target.files[0];
    if (!archivo) {        
        return;
    }

    $('#lblFileSelect').html('Archivo seleccionado');
    $('#lblLoad').show();

    var lector = new FileReader();

    lector.onload = function(e) {
      var contenido = e.target.result;      
      showContent(contenido);
      csvDocument = contenido;
    };
    lector.readAsText(archivo);
  }
  
  function showContent(contenido) {
    var ncont = JSON.stringify(contenido);
    var endText = ncont.indexOf("\\r\\");
    ncont = ncont.substring(1,endText);
    headFile = ncont;

    var elemento = document.getElementById('contenido-archivo');
    elemento.innerHTML = contenido;
  }
  
  document.getElementById('file-input').addEventListener('change', readFile, false);

  function pathFile(){
    alert(headFile);
  }

  function parseTableToJson(){
    
    Swal.fire({
        icon: 'success',
        title: 'Documento leido...',
        text: csvDocument
      })
  }

  async function SaveModel(){

    var table = $('#txtTable').val();
    var model = $('#txtModel').val();

    //Validación de los campos
    if(table == '' || table == undefined){
        Swal.fire({
            icon: 'error',
            title: 'Complete los registros',
            text: 'Debe escoger una tabla'
          })

          return;
    }
    else if(model == '' || model == undefined){
        Swal.fire({
            icon: 'error',
            title: 'Complete los registros',
            text: 'Debe darle un nombre a la plantilla'
          })

          return;
    }

    let currentDate = moment().format(); //new Date();    
    var resume_table = document.getElementById("CompareTableStructure");
    //let fileItem = [];
    let fileItem = "";    

    fileItem = '{"id":0, "Name":"' + $('#txtModel').val() + '","CreateDate":"' + currentDate + '","Enable":true,"Site":"' + table + '","JsonModel":[';

    for (var i = 0, row; row = resume_table.rows[i]; i++) {
        
        for (var j = 0, col; col = row.cells[j]; j++) {
            
            if(j == '1')
            {
                var sel = "#SelColFile" + i; 
                
                if($(sel).val() !== undefined) {
                    
                    if($(sel).val() !== '-- Seleccione un registro --'){
                        
                        fileItem = fileItem + '{"fieldFile":"' + $(sel).val() + '",';
                    } 
                    else{

                        fileItem = fileItem + '{"fieldFile":' + '"vacio",';
                    }                    
                }   
            }
            else
            {           
                if(col.innerText !== 'Campo de la tabla') {
                    ////fileItem.push(`campoBD:${col.innerText}` + '}');

                    fileItem = fileItem + `"fieldBD":"${col.innerText}` + '"},';
                }   
            }
        }
    }

    ////fileItem.push('}]');
    fileItem = fileItem.substring(0,fileItem.length - 1) + ']}';
    console.log(fileItem);

    var urlInsert = urlBase + "InsertModelFiles?ConnectionName=MysqlEngeni";
    
    let response = await fetch(urlInsert, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: fileItem
    })
    .then(response => response.json())
    .then(result =>  {
        JSON.stringify(result);

        $('#lblMarcNew').hide(); 

        Swal.fire({
            icon: 'success',
            title: result,
            text: 'Opercion exitosa!'
          })

          return;
    });
}

async function fnSearchModel(){
    
    var urlFile = urlBase + "GetModelFiles?ConnectionName=mysqlengeni";

    let response = await fetch(urlFile, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
    .then(response => response.json())
    .then(result =>  {

        $("#TableSearch tr td").remove(); 
        $('#lblTypeSearch').html("Plantillas");        

        for(let keyFile in result)
        {
            var newRow = document.createElement("tr");

            var newCell = document.createElement("td");
            newCell.innerHTML = result[keyFile].Name;        
            newRow.append(newCell);
            document.getElementById("TableSearchRows").appendChild(newRow);

            var newCell = document.createElement("td");
            newCell.innerHTML = result[keyFile].JsonModel;        
            newRow.append(newCell);
            document.getElementById("TableSearchRows").appendChild(newRow);
            newCell.style.display = "none";

            var newCell = document.createElement("td");
            newCell.innerHTML = result[keyFile].Site;        
            newRow.append(newCell);
            document.getElementById("TableSearchRows").appendChild(newRow);
            newCell.style.display = "none";
        }

        showHideTablesearch(1);
        
       
    })
    .catch((error) => {
        alert("No se pudo lograr conexión con la API, verifique que este disponible. Error: " + error);        
        return;
    });

    
}

async function fnTablesSearch(){

    var urlFile = urlBase + "GetAllTables?ConnectionName=Mysqlengeni";

    let response = await fetch(urlFile, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
    .then(response => response.json())
    .then(result =>  {

        $("#TableSearch tr td").remove(); 
        $('#lblTypeSearch').html("Tablas");

        for(let keyFile in result)
        {
            var newRow = document.createElement("tr");

            var newCell = document.createElement("td");
            newCell.innerHTML = result[keyFile].name;       
            newRow.append(newCell);
            document.getElementById("TableSearchRows").appendChild(newRow);
        }

        showHideTablesearch(1);
       
    })
    .catch((error) => {
        alert("No se pudo lograr conexión con la API, verifique que este disponible. Error: " + error);        
        return;
    });

    
}

$("#TableSearch").click(function(){  
        
    var tableSearch = document.getElementById('TableSearch');                
    var typeSel = $('#lblTypeSearch').html();

    for(var i = 1; i < tableSearch.rows.length; i++)
    {
        tableSearch.rows[i].onclick = function()
        {
            if(typeSel == 'Tablas'){
                $('#txtTable').val(this.cells[0].innerHTML); 
            }                
            if(typeSel == 'Plantillas'){       
                
                var jsonTxt = this.cells[1].innerHTML;

                $('#txtModel').val(this.cells[0].innerHTML); 
                //$('#lblJson').html(jsonTxt);                
                $('#txtTable').val(this.cells[2].innerHTML); 
                
                //Aquí debo hacer un split
                let jsonSplit = jsonTxt.split('{fieldBD:');

                valuesSelect = jsonSplit;

                for(var ij in jsonSplit){
                    var break_ = jsonSplit[ij].indexOf(',fieldFile:');
                    var col1 = jsonSplit[ij].substring(0,break_);
                    var col2 = jsonSplit[ij].substring(break_ + 11,jsonSplit[ij].length)
                    var break_2 = col2.indexOf('}');
                    col2 = col2.substring(0,break_2);
                    
                    var itemJson = jsonSplit[ij];
                }

                $('#lblMarcNew').hide();  
                $('#txtModel').prop('readonly', true);
                $('#loadSave').hide();
                $('#loadNew').show();  
                $("#CompareTableStructure tr td").remove();  
            }             
            
            showHideTablesearch(0);
        }  
        
    }     
    
})

function fnNewModel(){
    $('#txtModel').val('');
    $('#txtTable').val('');
    $('#txtModel').prop('readonly', false);
    $('#txtModel').focus();
    $('#lblMarcNew').show();
    $('#loadSave').show();
    $('#loadNew').hide();   
    valuesSelect = undefined;
    $("#CompareTableStructure tr td").remove(); 
}

function showHideTablesearch(val){
    if(val == 0)
        $('#ModalSearch').modal('hide');
    else
        $('#ModalSearch').modal('show');
}