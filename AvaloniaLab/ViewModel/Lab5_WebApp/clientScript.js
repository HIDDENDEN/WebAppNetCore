$(_ => { })

pageNumber = 0
maxPages = 0
document.getElementById("body").innerText = pageNumber

function AddOneElement(predictionStringResult,img)
{
    document.getElementById("list").innerHTML += "<li> <div>" + predictionStringResult + "<div/><img src=\"data:image.jpg;base64," + img + "\" width = 90 height = 90 />"
}

async function GetElements()
{
    try
    {
        let url = "http://localhost:5000/api/Images"
        
        var response = await fetch("http://localhost:5000/api/Images/"+pageNumber)
        var json = await response.json()
        maxPages =  Math.ceil(json.length/10)
        for (let i = 0; i < json.length;i++)
        {
            AddOneElement("Class: " + json[i].predictionStringResult + " Path: " + json[i].filePath , json[i].jpegImage )
        }

        if(json.length == 0)
            onClick_Previous()
        
        document.getElementById("body").innerText = pageNumber
    }
    catch (e)
    {
        window.alert(e)
    }
}

function onClickLeft()
{
    if(pageNumber > 0)
    {
        pageNumber--
        document.getElementById("list").innerHTML = ""
        GetElements()
    }
}

function onClickRight()
{
    if ((pageNumber+1)<=maxPages)
    {
    pageNumber++
    document.getElementById("list").innerHTML = ""
    GetElements()
    }
    else 
    {
        window.alert("No more available images in database")
    }
}

GetElements()