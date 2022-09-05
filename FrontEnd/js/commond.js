var headFile;

async function loadTable(){

    if(headFile == undefined){
        alert("no hay una ruta de archivo valida")
        return;
    }

    var tableS = $('#txtTable').val();
    var urlTable = "https://localhost:44346/GetTableStructure?ConnectionName=MySQLEngeni&TableName=" + tableS //Consulta de la estructura de la tabla
    
    let response = await fetch(urlTable, {
        method: 'GET'
    })
    .then(response => response.json())
    .then(result =>  {
            
            let headItemsFile = headFile.split(";");
            $("#CompareTableStructure tr td").remove(); 
            for(let key in result)
            {
                var TableName = JSON.stringify(result[key]);
 
                //console.log(TableName);

                var init = TableName.indexOf(':') + 2;                
                TableName = TableName.substring(init, TableName.length - 2);

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
                
                //$('#' + idSelect).empty();

                for(let itemAdd in headItemsFile)
                {
                    
                    addSelectOption(idSelect, headItemsFile[itemAdd]);
                }

            }            
        })
        .catch((error) => {
            alert('No se logró conectar al servicio, error: ' + error);
        });
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
    var urlFile = "https://localhost:44346/GetFileStructure?path=" + fileS;
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

function leerArchivo(e) {
    var archivo = e.target.files[0];
    if (!archivo) {
      return;
    }

    var lector = new FileReader();
    lector.onload = function(e) {
      var contenido = e.target.result;
      mostrarContenido(contenido);
    };
    lector.readAsText(archivo);
  }
  
  function mostrarContenido(contenido) {

    var ncont = JSON.stringify(contenido);
    var endText = ncont.indexOf("\\r\\");
    ncont = ncont.substring(1,endText);
    headFile = ncont;

    var elemento = document.getElementById('contenido-archivo');
    elemento.innerHTML = contenido;
  }
  
  document.getElementById('file-input').addEventListener('change', leerArchivo, false);

  function pathFile(){
    alert(headFile);
  }


  function parseTableToJson(){
    var obj = {};

    var table = $('#CompareTableStructure').val();

    //var row, rows = table.rows;
    
    console.log(table.rows);

    //console.log(rows);
    
    /*
    for (var i=0, iLen=rows.length; i<iLen; i++) {
        row = rows[i];
        obj[row.cells[0].textContent] = row.cells[1].textContent
    }
    alert(JSON.stringify(obj));
    */
  }