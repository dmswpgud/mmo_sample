using System.Collections;
using System.Collections.Generic;
using Client.Game.Map;
using GameServer;
using UnityEngine;

public class GameUtils : MonoBehaviour
{
    // // 오브젝트가 바라보는 전방의 타일을 반환.
    public static GridPoint GetUnitFrontTile(int currentX, int currentY, UnitDirection dir)
    {
        switch (dir)
        {
            case UnitDirection.UP:
                return new GridPoint(currentX, currentY + 1);
            case UnitDirection.UP_RIGHT:
                return new GridPoint(currentX + 1, currentY + 1);
            case UnitDirection.RIGHT:
                return new GridPoint(currentX + 1, currentY);
            case UnitDirection.DOWN_RIGHT:
                return new GridPoint(currentX + 1, currentY - 1);
            case UnitDirection.DOWN:
                return new GridPoint(currentX, currentY - 1);
            case UnitDirection.DOWN_LEFT:
                return new GridPoint(currentX - 1, currentY - 1);
            case UnitDirection.LEFT:
                return new GridPoint(currentX - 1, currentY);
            case UnitDirection.UP_LEFT:
                return new GridPoint(currentX - 1, currentY + 1);
        }
        return null;
    }
    
    public static UnitDirection SetDirectionByPosition(int currentX, int currentY, int destX, int destY)
    {
        if (currentX > destX && currentY > destY)
        {
            return UnitDirection.DOWN_LEFT; //
        }
        if (currentX > destX && currentY == destY)
        {
            return UnitDirection.LEFT; //
        }
        if (currentX > destX && currentY < destY)
        {
            return UnitDirection.UP_LEFT; //
        }
        if (currentX == destX && currentY < destY)
        {
            return UnitDirection.UP; //
        }
        if (currentX < destX && currentY < destY)
        {
            return UnitDirection.UP_RIGHT; //
        }
        if (currentX < destX && currentY == destY)
        {
            return UnitDirection.RIGHT; //
        }
        if (currentX < destX && currentY > destY)
        {
            return UnitDirection.DOWN_RIGHT;
        }
        if (currentX == destX && currentY > destY)
        {
            return UnitDirection.DOWN; //
        }

        return UnitDirection.UP;
    }

}
