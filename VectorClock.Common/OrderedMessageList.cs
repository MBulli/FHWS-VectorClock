using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class OrderedMessageList : IEnumerable
    {
        public List<Message> delayedMessageList = new List<Message>();
        public int Count = 0;

        public void PushItem(Message msg)
        {
            delayedMessageList.Add(msg);
            OrderList();
            Count = delayedMessageList.Count;
        }

        public Message PopItem()
        {
            Message returnMsg = delayedMessageList[0];
            delayedMessageList.RemoveAt(0);
            Count = delayedMessageList.Count;
            return returnMsg;
        }

        private void OrderList()
        {
            bool swapped = false;
            Message saveMessage = null;

            while(swapped == true)
            {
                for(int i = 0; i < delayedMessageList.Count - 1; i++)
                {
                    if(delayedMessageList[i].communicationBlock.clock.Compare(delayedMessageList[i + 1].communicationBlock.clock) == ComparisonResult.After)
                    {
                        saveMessage = delayedMessageList[i];
                        delayedMessageList[i] = delayedMessageList[i + 1];
                        delayedMessageList[i + 1] = saveMessage;
                    }
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return delayedMessageList.GetEnumerator();
        }
    }
}
