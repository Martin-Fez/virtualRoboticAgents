using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridMaker : MonoBehaviour
{
    public GameObject GridSquare;

    public int gridSizeX;
    public int gridSizeZ;
    public int scale;

    public Transform[] gridPoints; // Array of GridsPoints


    void Start()
    {
        if(scale <= 0)
        {
            scale = 1;
        }


        gridSizeX = gridSizeX * scale;
        gridSizeZ = gridSizeZ * scale;

        Vector3 startingPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        float GridSquareSize = GridSquare.transform.localScale.x/scale;
        int IDindex = 0;

        gridPoints = new Transform[gridSizeX* gridSizeZ];

        for (int i = 0; i < gridSizeZ; i++)
        {
            for (int j = 0; j < gridSizeX; j++)
            {
                Vector3 newGridPosition = new Vector3(startingPos.x + j * GridSquareSize, startingPos.y, startingPos.z+ i * GridSquareSize);

                var newSquare = Instantiate(GridSquare, newGridPosition, Quaternion.identity);

                newSquare.transform.parent = gameObject.transform;
                newSquare.transform.localScale = new Vector3(GridSquare.transform.localScale.x / scale, GridSquare.transform.localScale.y, GridSquare.transform.localScale.z / scale);


                newSquare.GetComponent<GridLogic>().GridID = IDindex;
                gridPoints[IDindex] = newSquare.transform;
                IDindex++;

            }
        }




    }
    void Update()
    {
        
    }
}
