using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualKeyScript : IndividualClass
{
    public IEnumerator UpdateBackTypeAndImage(BackColorTypes colorType, float updateImageDelay = 0f)
    {
        currentBoxBack = colorType;
        yield return new WaitForSeconds(updateImageDelay);
        UpdateBackImage(ComparisonColors[colorType]);
    }
}
