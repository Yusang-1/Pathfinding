using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Controller
{
    public class InputOnUIJudgeBuffer<TCommandType>
        where TCommandType : struct, Enum
    {
        private readonly Queue<BufferedCommand<TCommandType>> buffer = new();

        public void Enqueue(BufferedCommand<TCommandType> command)
        {
            buffer.Enqueue(command);
        }

        public void LateUpdate()
        {
            while (buffer.Count > 0)
            {
                var command = buffer.Dequeue();
                command.Target.ExecuteBufferedCommand(command);
            }
        }
    }

    public readonly struct BufferedCommand<TCommandType>
        where TCommandType : struct, Enum
    {        
        public readonly IHaveTouchBuffer<TCommandType> Target;
        public readonly TCommandType Type;
        public readonly int TouchId;
        public readonly Vector2 Position;
        public readonly bool ShiftPressed;

        public BufferedCommand(            
            IHaveTouchBuffer<TCommandType> target,
            TCommandType type,
            int touchId = 0,
            Vector2 position = default,
            bool shiftPressed = false)
        {
            Target = target;
            Type = type;
            TouchId = touchId;
            Position = position;
            ShiftPressed = shiftPressed;
        }
    }

    public interface IHaveTouchBuffer<TCommandType>
        where TCommandType : struct, Enum
    {
        public void ExecuteBufferedCommand(BufferedCommand<TCommandType> bufferedCommand);
    }

    public interface IHaveUnitTouchBuffer : IHaveTouchBuffer<IHaveUnitTouchBuffer.CommandType>
    {
        public enum CommandType
        {
            CheckTouch0UI,
            JudgeScreenMoveOrDrag,
            SelectOrMove
        }

        public void InitializeCommandTypeActionDict();
        public void CheckTouch0IsOverGameObject(BufferedCommand<CommandType> bufferedCommand);
        public void JudgeScreenMoveOrDrag(BufferedCommand<CommandType> bufferedCommand);
        public void SelectOrMove(BufferedCommand<CommandType> bufferedCommand);
    }
    
    public interface IHavePlayerTouchBuffer : IHaveTouchBuffer<IHavePlayerTouchBuffer.CommandType>
    {
        public enum CommandType
        {
            CheckTouch0UI,
            JudgeScreenMoveOrDrag,
            Select
        }
        
        public void InitializeCommandTypeActionDict();
        public void CheckTouch0IsOverGameObject(BufferedCommand<CommandType> bufferedCommand);
        public void JudgeScreenMoveOrDrag(BufferedCommand<CommandType> bufferedCommand);
        public void SelectUnit(BufferedCommand<CommandType> bufferedCommand);
    }
}
